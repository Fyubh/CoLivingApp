import Foundation
import Observation

/// Backs `ServicesView` (Maintenance section). Holds the list of the user's
/// requests, plus the apartment context so the create-form can pre-fill
/// location ids without a second store dependency.
///
/// CRUD ops re-fetch the list after the workflow call returns — cheaper
/// than splicing the optimistic update across status transitions, and the
/// list is small. SignalR `MaintenanceStatusChanged` push isn't wired yet
/// (Phase 8); pull-to-refresh covers async status flips for now.
@Observable
final class MaintenanceStore {
    private(set) var apartment: MyApartmentContextDto?
    private(set) var requests: [MaintenanceRequestDto] = []
    private(set) var isLoading: Bool = false
    private(set) var hasLoaded: Bool = false
    /// In-flight workflow op (create / cancel / rate). Drives sheet button
    /// disabled state so the user can't double-tap.
    private(set) var isSubmitting: Bool = false
    var errorMessage: String?

    private let api: APIClient
    private let auth: AuthStore

    init(api: APIClient = .shared, auth: AuthStore) {
        self.api = api
        self.auth = auth
    }

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
            async let list: [MaintenanceRequestDto] = auth.authedCall { token in
                try await self.api.getMyMaintenanceRequests(token: token)
            }
            self.apartment = try await apt
            self.requests = try await list
            self.hasLoaded = true
        } catch APIError.unauthorized {
            // AuthFlow tearing down.
        } catch {
            errorMessage = friendlyMessage(for: error)
        }
    }

    // MARK: - Workflow ops

    /// Returns `true` on success so the sheet can dismiss itself; on
    /// failure leaves `errorMessage` set for the caller to surface.
    func create(_ payload: CreateMaintenancePayload) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        do {
            _ = try await auth.authedCall { token in
                try await self.api.createMaintenanceRequest(payload: payload, token: token)
            }
            await refresh()
            return true
        } catch APIError.unauthorized {
            return false
        } catch {
            errorMessage = friendlyMessage(for: error)
            return false
        }
    }

    func cancel(id: UUID, reason: String?) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        do {
            try await auth.authedCall { token in
                try await self.api.cancelMaintenanceRequest(
                    id: id, reason: reason, token: token
                )
            }
            await refresh()
            return true
        } catch APIError.unauthorized {
            return false
        } catch {
            errorMessage = friendlyMessage(for: error)
            return false
        }
    }

    func rate(id: UUID, rating: Int, feedback: String?) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        do {
            try await auth.authedCall { token in
                try await self.api.rateMaintenanceRequest(
                    id: id, rating: rating, feedback: feedback, token: token
                )
            }
            await refresh()
            return true
        } catch APIError.unauthorized {
            return false
        } catch {
            errorMessage = friendlyMessage(for: error)
            return false
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
