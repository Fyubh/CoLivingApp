import Foundation
import Observation

/// Backs the Уборка pane. Single chore list per apartment plus three
/// workflow ops — complete (assignee marks done), confirm (peer approves),
/// reject (peer disputes; server creates a €5 penalty expense).
@Observable
final class ChoresStore {
    private(set) var chores: [ChoreDto] = []
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
            self.chores = try await auth.authedCall { token in
                try await self.api.getChores(apartmentId: self.apartment.apartmentId, token: token)
            }
            self.hasLoaded = true
        } catch APIError.unauthorized {
            // AuthFlow tearing down.
        } catch {
            errorMessage = friendlyMessage(for: error)
        }
    }

    // MARK: - Workflow ops

    func create(
        title: String,
        description: String?,
        category: ChoreCategory,
        assignedUserId: String?,
        dueDate: Date?
    ) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        let payload = CreateChorePayload(
            apartmentId: apartment.apartmentId,
            title: title,
            description: description,
            category: category,
            assignedUserId: assignedUserId,
            dueDate: dueDate
        )

        do {
            _ = try await auth.authedCall { token in
                try await self.api.createChore(payload: payload, token: token)
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

    func complete(choreId: UUID) async -> Bool {
        await runAction { token in
            try await self.api.completeChore(
                choreId: choreId,
                apartmentId: self.apartment.apartmentId,
                token: token
            )
        }
    }

    func confirm(choreId: UUID) async -> Bool {
        await runAction { token in
            try await self.api.confirmChore(
                choreId: choreId,
                apartmentId: self.apartment.apartmentId,
                token: token
            )
        }
    }

    func reject(choreId: UUID) async -> Bool {
        await runAction { token in
            try await self.api.rejectChore(
                choreId: choreId,
                apartmentId: self.apartment.apartmentId,
                token: token
            )
        }
    }

    private func runAction(_ block: @escaping (String) async throws -> Void) async -> Bool {
        guard !isSubmitting else { return false }
        isSubmitting = true
        errorMessage = nil
        defer { isSubmitting = false }

        do {
            try await auth.authedCall(block)
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
