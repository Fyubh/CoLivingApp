import Foundation
import Observation

/// Backs `HomeView`. Owns the apartment context and notifications list,
/// surfaces a single `errorMessage` for the screen to render, and routes
/// every authed call through `AuthStore.authedCall` so a stale token folds
/// the user back to Login automatically.
///
/// Lifecycle: created once in `HomeView` as `@State` — `TabView` keeps the
/// view alive across tab switches, so the store and its data survive.
@Observable
final class HomeStore {
    private(set) var apartment: MyApartmentContextDto?
    private(set) var notifications: [ResidentNotificationDto] = []
    /// Top N Available inventory items for the Home «Покупки» preview.
    /// Fail-soft: stays empty if the secondary fetch errors so the screen
    /// still renders apartment + notifications.
    private(set) var inventoryPreview: [ItemDto] = []
    /// Earliest-due pending chore, surfaced on Home as «Ближайшая уборка».
    /// Nil when there are no pending chores or the secondary fetch errored.
    private(set) var nextChore: ChoreDto?
    private(set) var isLoading: Bool = false
    /// Flips to `true` after the first successful refresh, regardless of
    /// whether `apartment` came back non-nil. Lets HomeView distinguish
    /// "still loading" from "loaded and the user has no apartment".
    private(set) var hasLoaded = false
    var errorMessage: String?

    private static let inventoryPreviewLimit = 5

    private let api: APIClient
    private let auth: AuthStore

    init(api: APIClient = .shared, auth: AuthStore) {
        self.api = api
        self.auth = auth
    }

    /// Idempotent first-load — `HomeView.task` calls this on appear; only
    /// the first call hits the network. Pull-to-refresh uses `refresh()`.
    func bootstrap() async {
        guard !hasLoaded else { return }
        await refresh()
    }

    func refresh() async {
        isLoading = true
        errorMessage = nil
        defer { isLoading = false }

        do {
            async let apt: MyApartmentContextDto? = auth.authedCall { token in
                try await self.api.getMyApartmentContext(token: token)
            }
            async let notif: [ResidentNotificationDto] = auth.authedCall { token in
                try await self.api.getMyNotifications(token: token)
            }
            self.apartment = try await apt
            self.notifications = try await notif

            // Secondary fetches: only meaningful when we have an apartment.
            // Run in parallel with each other; failures don't poison the
            // Home screen — the relevant section just stays empty.
            if let apartmentId = self.apartment?.apartmentId {
                async let inv = self.fetchInventoryPreview(apartmentId: apartmentId)
                async let chr = self.fetchNextChore(apartmentId: apartmentId)
                self.inventoryPreview = await inv
                self.nextChore = await chr
            } else {
                self.inventoryPreview = []
                self.nextChore = nil
            }

            self.hasLoaded = true
        } catch APIError.unauthorized {
            // AuthStore already signed out; AuthFlow will rebuild to Login.
        } catch {
            errorMessage = friendlyMessage(for: error)
        }
    }

    /// Optimistic mark-as-read: flip the local row first so the UI reacts
    /// immediately, roll back on network failure. The `notificationId` in
    /// the response is unused — we already know the id.
    func markRead(id: UUID) async {
        guard let idx = notifications.firstIndex(where: { $0.id == id }) else { return }
        let prev = notifications[idx]
        guard !prev.isRead else { return }

        notifications[idx] = ResidentNotificationDto(
            id: prev.id,
            buildingId: prev.buildingId,
            title: prev.title,
            body: prev.body,
            isImportant: prev.isImportant,
            isRead: true,
            createdAt: prev.createdAt,
            readAt: Date()
        )

        do {
            try await auth.authedCall { token in
                try await self.api.markNotificationRead(id: id, token: token)
            }
        } catch APIError.unauthorized {
            // signed out — list will be torn down with the view tree
        } catch {
            // Rollback if the row is still where we left it.
            if let cur = notifications.firstIndex(where: { $0.id == id }) {
                notifications[cur] = prev
            }
            errorMessage = friendlyMessage(for: error)
        }
    }

    // MARK: - Secondary fetches (fail-soft)

    private func fetchInventoryPreview(apartmentId: UUID) async -> [ItemDto] {
        do {
            let items = try await auth.authedCall { token in
                try await self.api.getInventoryItems(
                    apartmentId: apartmentId,
                    status: .available,
                    token: token
                )
            }
            return Array(items.prefix(Self.inventoryPreviewLimit))
        } catch {
            return []
        }
    }

    private func fetchNextChore(apartmentId: UUID) async -> ChoreDto? {
        do {
            let chores = try await auth.authedCall { token in
                try await self.api.getChores(apartmentId: apartmentId, token: token)
            }
            return Self.pickNextChore(from: chores)
        } catch {
            return nil
        }
    }

    /// Earliest-due pending chore. Chores without a `dueDate` sort after
    /// dated ones so the user always sees the most time-sensitive item.
    private static func pickNextChore(from chores: [ChoreDto]) -> ChoreDto? {
        return chores
            .filter { $0.choreStatus == .pending }
            .min { lhs, rhs in
                switch (lhs.dueDate, rhs.dueDate) {
                case let (l?, r?): return l < r
                case (_?, nil):    return true
                case (nil, _?):    return false
                case (nil, nil):   return false
                }
            }
    }

    private func friendlyMessage(for error: Error) -> String {
        if let api = error as? APIError, let desc = api.errorDescription {
            return desc
        }
        return error.localizedDescription.isEmpty
            ? "Не удалось загрузить."
            : error.localizedDescription
    }
}
