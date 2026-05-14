import Foundation
import Observation

/// Стор чата здания. Загрузка истории + send + три moderation-операции
/// (delete-own / report / block, общие с apartment-чатом). Подписка на
/// SignalR live-апдейты через `RealtimeService` — новые сообщения от других
/// жильцов появляются без pull-to-refresh.
///
/// Своё отправленное сообщение тоже прилетит обратно через SignalR — поэтому
/// дедупим по `id`, иначе у автора появятся дубликаты.
@MainActor
@Observable
final class BuildingChatStore {
    private(set) var messages: [ChatMessageDto] = []
    private(set) var isLoading: Bool = false
    private(set) var hasLoaded: Bool = false
    private(set) var isSending: Bool = false
    var errorMessage: String?

    let buildingId: UUID
    let currentUserId: String

    private let api: APIClient
    private let auth: AuthStore
    private let realtime: RealtimeService
    private var subscription: RealtimeSubscription?

    init(
        buildingId: UUID,
        currentUserId: String,
        api: APIClient = .shared,
        auth: AuthStore,
        realtime: RealtimeService
    ) {
        self.buildingId = buildingId
        self.currentUserId = currentUserId
        self.api = api
        self.auth = auth
        self.realtime = realtime
    }

    func bootstrap() async {
        if subscription == nil {
            subscription = realtime.subscribeBuildingChat(buildingId: buildingId) { [weak self] dto in
                self?.handleIncoming(dto)
            }
        }
        guard !hasLoaded else { return }
        await refresh()
    }

    func refresh() async {
        isLoading = true
        errorMessage = nil
        defer { isLoading = false }

        do {
            self.messages = try await auth.authedCall { token in
                try await self.api.getBuildingChatHistory(buildingId: self.buildingId, token: token)
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
                try await self.api.sendBuildingChatMessage(
                    buildingId: self.buildingId,
                    text: trimmed,
                    token: token
                )
            }
            // Live-апдейт прилетит через SignalR; refresh не нужен.
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

    private func handleIncoming(_ dto: ChatMessageDto) {
        // Дедуп по id — сервер также пушит автору обратно через SignalR.
        if messages.contains(where: { $0.id == dto.id }) { return }
        messages.append(dto)
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
