import Foundation
import Observation

/// Backs the Финансы pane inside `NeighborsView`. Holds two parallel
/// resources from the backend — the apartment-scoped expense feed and the
/// per-roommate net balance vs. the current user — plus a single
/// `isSubmitting` guard for create / settle.
///
/// Lives one level below `NeighborsStore`: the apartment context is fetched
/// once at the tab root and handed to this store at init, so we don't
/// re-fetch `my-context` here. Re-runs both list calls after every workflow
/// op (cheaper than splicing optimistic updates across two collections that
/// share state on the server).
@Observable
final class FinanceStore {
    private(set) var expenses: [ExpenseDto] = []
    private(set) var balances: [UserBalanceDto] = []
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
            async let exp: [ExpenseDto] = auth.authedCall { token in
                try await self.api.getExpenses(apartmentId: self.apartment.apartmentId, token: token)
            }
            async let bal: [UserBalanceDto] = auth.authedCall { token in
                try await self.api.getBalance(apartmentId: self.apartment.apartmentId, token: token)
            }
            self.expenses = try await exp
            self.balances = try await bal
            self.hasLoaded = true
        } catch APIError.unauthorized {
            // AuthFlow tearing down.
        } catch {
            errorMessage = friendlyMessage(for: error)
        }
    }

    // MARK: - Workflow ops

    func createExpense(
        amount: Decimal,
        description: String,
        category: ExpenseCategory
    ) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        let payload = CreateExpensePayload(
            apartmentId: apartment.apartmentId,
            amount: amount,
            description: description,
            category: category
        )

        do {
            _ = try await auth.authedCall { token in
                try await self.api.createExpense(payload: payload, token: token)
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

    func settleDebt(receiverId: String, amount: Decimal) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        let payload = SettleDebtPayload(
            apartmentId: apartment.apartmentId,
            receiverId: receiverId,
            amount: amount
        )

        do {
            _ = try await auth.authedCall { token in
                try await self.api.settleDebt(payload: payload, token: token)
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
