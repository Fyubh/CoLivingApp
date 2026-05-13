import Foundation
import Observation

/// Контекст для Сообщество tab — то же `MyApartmentContextDto` что у Соседей,
/// но фокус на `buildingId` (для чата здания) и `meUserId`. Если у юзера
/// нет квартиры → нет здания → нет чата (вкладка покажет empty-state).
@Observable
final class CommunityStore {
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
            // AuthFlow tearing down.
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
