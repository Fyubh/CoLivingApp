import Foundation
import Observation

/// Backs `ProfileView`. Holds the user's `me` (identity, role) and the same
/// apartment context HomeStore renders — Profile re-fetches independently
/// instead of plumbing the store across tabs. Two parallel GETs per first
/// load is cheap and avoids the fragility of cross-tab state.
///
/// Lifecycle mirrors HomeStore: created once in `ProfileView` as `@State`,
/// `bootstrap()` is idempotent, `refresh()` is the pull-to-refresh entry
/// point. 401 from either call routes through `AuthStore.signOut()` so the
/// AuthFlow re-renders to Login.
@Observable
final class ProfileStore {
    private(set) var me: MeDto?
    private(set) var apartment: MyApartmentContextDto?
    private(set) var isLoading: Bool = false
    /// Flips to `true` after the first successful refresh. Used by the view
    /// to distinguish "still loading" from "loaded with no apartment".
    private(set) var hasLoaded: Bool = false
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
            async let me: MeDto = auth.authedCall { token in
                try await self.api.getMe(token: token)
            }
            async let apt: MyApartmentContextDto? = auth.authedCall { token in
                try await self.api.getMyApartmentContext(token: token)
            }
            self.me = try await me
            self.apartment = try await apt
            self.hasLoaded = true
        } catch APIError.unauthorized {
            // AuthStore signed out; AuthFlow tears the view tree down.
        } catch {
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
