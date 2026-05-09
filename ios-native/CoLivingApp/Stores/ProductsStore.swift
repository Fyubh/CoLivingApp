import Foundation
import Observation

/// Backs the Покупки pane. MVP scope: single bucket of `Available` items —
/// what's currently in the apartment. Two workflow ops: consume (used up)
/// and remove (mistake). InCart / Checkout are intentionally not exposed
/// (see InventoryDTOs.swift for rationale).
@Observable
final class ProductsStore {
    private(set) var items: [ItemDto] = []
    private(set) var isLoading: Bool = false
    private(set) var hasLoaded: Bool = false
    private(set) var isSubmitting: Bool = false
    var errorMessage: String?

    private let apartment: MyApartmentContextDto
    private let api: APIClient
    private let auth: AuthStore

    init(apartment: MyApartmentContextDto, api: APIClient = .shared, auth: AuthStore) {
        self.apartment = apartment
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
            self.items = try await auth.authedCall { token in
                try await self.api.getInventoryItems(
                    apartmentId: self.apartment.apartmentId,
                    status: .available,
                    token: token
                )
            }
            self.hasLoaded = true
        } catch APIError.unauthorized {
            // AuthFlow tearing down.
        } catch {
            errorMessage = friendlyMessage(for: error)
        }
    }

    // MARK: - Workflow ops

    func createItem(
        name: String,
        quantity: Decimal,
        unit: UnitType,
        category: ItemCategory,
        location: StorageLocation
    ) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        let payload = CreateItemPayload(
            apartmentId: apartment.apartmentId,
            customName: name,
            quantity: quantity,
            unit: unit,
            category: category,
            location: location
        )

        do {
            _ = try await auth.authedCall { token in
                try await self.api.createInventoryItem(payload: payload, token: token)
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

    func consume(itemId: UUID) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        do {
            try await auth.authedCall { token in
                try await self.api.consumeInventoryItem(
                    itemId: itemId,
                    apartmentId: self.apartment.apartmentId,
                    token: token
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

    func remove(itemId: UUID) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        do {
            try await auth.authedCall { token in
                try await self.api.removeInventoryItem(
                    itemId: itemId,
                    apartmentId: self.apartment.apartmentId,
                    token: token
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
