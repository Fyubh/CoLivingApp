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
    private(set) var isLoading: Bool = false
    var errorMessage: String?

    private let api: APIClient
    private let auth: AuthStore
    private var hasLoaded = false

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
            async let apt: MyApartmentContextDto = auth.authedCall { token in
                try await self.api.getMyApartmentContext(token: token)
            }
            async let notif: [ResidentNotificationDto] = auth.authedCall { token in
                try await self.api.getMyNotifications(token: token)
            }
            self.apartment = try await apt
            self.notifications = try await notif
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

    private func friendlyMessage(for error: Error) -> String {
        if let api = error as? APIError, let desc = api.errorDescription {
            return desc
        }
        return error.localizedDescription.isEmpty
            ? "Не удалось загрузить."
            : error.localizedDescription
    }
}
