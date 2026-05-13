import Foundation
import Observation

/// Подписка на live-сообщения через `RealtimeService`. Стор регистрирует
/// callback и должен удерживать токен живым (через `@State`), иначе сервис
/// размотает подписку. На weak-ссылку токен не превратится — это сделано
/// намеренно, чтобы можно было unsubscribe детерминированно при смене скоупа
/// (например, при logout).
@MainActor
final class RealtimeSubscription {
    let id: UUID = UUID()
    fileprivate weak var owner: RealtimeService?
    init(owner: RealtimeService) { self.owner = owner }
    deinit {
        let cancelOwner = owner
        let cancelId = id
        Task { @MainActor in
            cancelOwner?.unsubscribe(id: cancelId)
        }
    }
}

/// Тонкий фасад над `SignalRClient` для приложения. Стартует один раз после
/// логина, держит одно WS-соединение и роутит ReceiveBuildingChatMessage в
/// зарегистрированные callbacks. После reconnect автоматически рестартит
/// присоединение к буцилдинг-группе.
///
/// В 8.1 — только building-чат. Apartment-чат остаётся на pull-to-refresh
/// (не ломаем работающее, расширим в 8.2+ по необходимости).
@MainActor
@Observable
final class RealtimeService {
    private let api: APIClient
    private var signalR: SignalRClient?

    /// Группы, в которые нужно (пере)джойниться после каждого connect.
    private var joinedBuildingId: UUID?

    /// Подписчики на ReceiveBuildingChatMessage. Ключ — id подписки,
    /// значение — handler. Подписка живёт пока жив `RealtimeSubscription`-токен.
    private var buildingHandlers: [UUID: (ChatMessageDto) -> Void] = [:]

    init(api: APIClient = .shared) {
        self.api = api
    }

    func start(auth: AuthStore) {
        guard signalR == nil else { return }

        let client = SignalRClient(
            hubURL: api.hubURL,
            tokenProvider: { [weak auth] in
                await MainActor.run { auth?.currentToken }
            }
        )
        signalR = client

        Task {
            await client.on("ReceiveBuildingChatMessage") { [weak self] data in
                guard let dto = Self.decodeArgs(data, as: ChatMessageDto.self) else { return }
                await MainActor.run { self?.dispatchBuilding(dto) }
            }
            await client.onConnect { [weak self] in
                await self?.rejoinGroups()
            }
            await client.start()
        }
    }

    func stop() {
        let client = signalR
        signalR = nil
        joinedBuildingId = nil
        buildingHandlers.removeAll()
        Task { await client?.stop() }
    }

    /// Подписка стора на ReceiveBuildingChatMessage. Возвращает токен — стор
    /// держит его в @State; когда вьюшка размонтируется, токен дропается,
    /// callback автоматически снимается.
    func subscribeBuildingChat(buildingId: UUID, handler: @escaping (ChatMessageDto) -> Void) -> RealtimeSubscription {
        if joinedBuildingId != buildingId {
            joinedBuildingId = buildingId
            Task { [signalR] in
                await signalR?.invoke(target: "JoinBuildingGroup", arguments: [buildingId.uuidString])
            }
        }
        let token = RealtimeSubscription(owner: self)
        buildingHandlers[token.id] = handler
        return token
    }

    fileprivate func unsubscribe(id: UUID) {
        buildingHandlers.removeValue(forKey: id)
    }

    private func dispatchBuilding(_ dto: ChatMessageDto) {
        for handler in buildingHandlers.values {
            handler(dto)
        }
    }

    private func rejoinGroups() async {
        guard let signalR else { return }
        if let id = joinedBuildingId {
            await signalR.invoke(target: "JoinBuildingGroup", arguments: [id.uuidString])
        }
    }

    /// Серверный invocation приходит как JSON-массив аргументов: `[{...}]`.
    /// Достаём первый элемент и декодируем в DTO.
    private static func decodeArgs<T: Decodable>(_ data: Data, as type: T.Type) -> T? {
        guard let array = try? JSONSerialization.jsonObject(with: data) as? [Any],
              let first = array.first,
              let firstData = try? JSONSerialization.data(withJSONObject: first) else {
            return nil
        }
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in
            let str = try decoder.singleValueContainer().decode(String.self)
            let f = ISO8601DateFormatter()
            f.formatOptions = [.withInternetDateTime, .withFractionalSeconds]
            if let d = f.date(from: str) { return d }
            f.formatOptions = [.withInternetDateTime]
            if let d = f.date(from: str) { return d }
            return Date()
        }
        return try? decoder.decode(T.self, from: firstData)
    }
}
