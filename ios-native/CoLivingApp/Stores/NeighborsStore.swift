import Foundation
import Observation

/// Backs the Соседи tab shell. Single fetch of `/Apartments/my-context`
/// (apartment id + roommates) so the three sub-stores — Finance / Products
/// / Cleaning — can take that context at init and run their own list calls
/// without refetching the same membership lookup.
///
/// All three Phase 6 backend modules (Expenses, Inventory, Chores) require
/// `apartmentId` from the client — none of them auto-scope by JWT the way
/// `/Maintenance/my` does — so this store is the only place `my-context`
/// gets called for the Соседи tab.
@Observable
final class NeighborsStore {
    private(set) var apartment: MyApartmentContextDto?
    private(set) var isLoading: Bool = false
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
            self.apartment = try await auth.authedCall { token in
                try await self.api.getMyApartmentContext(token: token)
            }
            self.hasLoaded = true
        } catch APIError.unauthorized {
            // AuthFlow is tearing down — let it.
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
