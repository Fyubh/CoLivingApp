import Foundation
import Observation

/// Backs the Чат pane. История + send + три moderation-операции
/// (delete-own / report / block). Без SignalR в MVP — обновляемся через
/// pull-to-refresh и пост-операционный refresh. После delete и block
/// перезагружаем историю целиком: это самый честный способ убрать
/// удалённое/скрытое сообщение из ленты, потому что фильтр живёт на сервере.
@Observable
final class ChatStore {
    private(set) var messages: [ChatMessageDto] = []
    private(set) var isLoading: Bool = false
    private(set) var hasLoaded: Bool = false
    private(set) var isSending: Bool = false
    var errorMessage: String?

    private let apartment: MyApartmentContextDto
    private let api: APIClient
    private let auth: AuthStore

    init(apartment: MyApartmentContextDto, api: APIClient = .shared, auth: AuthStore) {
        self.apartment = apartment
        self.api = api
        self.auth = auth
    }

    var currentUserId: String { apartment.meUserId }

    func bootstrap() async {
        guard !hasLoaded else { return }
        await refresh()
    }

    func refresh() async {
        isLoading = true
        errorMessage = nil
        defer { isLoading = false }

        do {
            self.messages = try await auth.authedCall { token in
                try await self.api.getChatHistory(apartmentId: self.apartment.apartmentId, token: token)
            }
            self.hasLoaded = true
        } catch APIError.unauthorized {
            // AuthFlow tearing down.
        } catch {
            errorMessage = friendlyMessage(for: error)
        }
    }

    func send(text: String) async -> Bool {
        let trimmed = text.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty, !isSending else { return false }
        isSending = true
        errorMessage = nil
        defer { isSending = false }

        do {
            try await auth.authedCall { token in
                try await self.api.sendChatMessage(
                    apartmentId: self.apartment.apartmentId,
                    text: trimmed,
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

    func deleteOwn(messageId: UUID) async -> Bool {
        await runModeration { token in
            try await self.api.deleteChatMessage(messageId: messageId, token: token)
        }
    }

    func report(messageId: UUID, reason: String?) async -> Bool {
        await runModeration { token in
            try await self.api.reportChatMessage(messageId: messageId, reason: reason, token: token)
        }
    }

    func block(userId: String) async -> Bool {
        await runModeration { token in
            try await self.api.blockChatUser(blockedUserId: userId, token: token)
        }
    }

    private func runModeration(_ block: @escaping (String) async throws -> Void) async -> Bool {
        errorMessage = nil
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
