import Foundation

/// Минимальный SignalR-клиент под наш CoLivingHub: JSON-протокол, WebSocket-
/// транспорт, ровно те операции, которые нужны (invocation client→server,
/// получение invocation server→client, ping/keepalive). Покрывает 95% сценариев
/// без зависимости на сторонний пакет — этого достаточно для 8.1 (apartment +
/// building chat). Если в 8.5 захочется бинарный протокол / MessagePack —
/// заменим целиком.
///
/// Поток сообщений в JSON-протоколе:
///   * Клиент после WS-handshake'а шлёт `{"protocol":"json","version":1}<RS>`,
///     где RS — байт 0x1E (Record Separator). Без него сервер не понимает, что
///     сообщение закончилось.
///   * Дальше любое сообщение — JSON-объект, заканчивающийся 0x1E.
///   * Invocation client→server: `{"type":1,"target":"...","arguments":[...]}`
///   * Invocation server→client: то же самое; мы вызываем зарегистрированный
///     handler по `target`.
///   * Keepalive: сервер шлёт `{"type":6}` ~раз в 15с; клиент отвечает таким же.
actor SignalRClient {
    enum State {
        case disconnected
        case connecting
        case connected
        case reconnecting
    }

    private let hubURL: URL
    private let tokenProvider: @Sendable () async -> String?

    private var session: URLSession
    private var task: URLSessionWebSocketTask?
    private var state: State = .disconnected

    /// Подписчики на server-to-client invocation. Ключ — `target`, значение —
    /// массив handler'ов, которые получают сырой JSON-массив аргументов.
    private var handlers: [String: [(Data) async -> Void]] = [:]

    /// Колбэки на (re)connect — для джойна групп после успешного хендшейка.
    private var onConnected: [@Sendable () async -> Void] = []

    private var receiveTask: Task<Void, Never>?
    private var keepaliveTask: Task<Void, Never>?
    private var reconnectTask: Task<Void, Never>?

    /// 0x1E — разделитель сообщений в SignalR JSON-протоколе.
    private static let recordSeparator: UInt8 = 0x1E

    init(hubURL: URL, tokenProvider: @escaping @Sendable () async -> String?) {
        self.hubURL = hubURL
        self.tokenProvider = tokenProvider
        self.session = URLSession(configuration: .default)
    }

    // MARK: - Public API

    func on(_ target: String, handler: @escaping (Data) async -> Void) {
        handlers[target, default: []].append(handler)
    }

    func onConnect(_ block: @escaping @Sendable () async -> Void) {
        onConnected.append(block)
    }

    func start() async {
        guard state == .disconnected else { return }
        await connect()
    }

    func stop() async {
        reconnectTask?.cancel()
        reconnectTask = nil
        await tearDown()
        state = .disconnected
    }

    /// Вызов hub-метода. Без ожидания ответа (`type=1` без `invocationId`).
    /// Если соединения нет — тихо пропускаем; клиент догонит после reconnect
    /// (handlers в `onConnect` повторят джойн групп).
    func invoke(target: String, arguments: [Any]) async {
        guard state == .connected, let task else { return }
        let payload: [String: Any] = [
            "type": 1,
            "target": target,
            "arguments": arguments
        ]
        guard let data = try? JSONSerialization.data(withJSONObject: payload, options: []) else { return }
        await send(frame: data)
    }

    // MARK: - Connect / handshake

    private func connect() async {
        state = .connecting

        guard let token = await tokenProvider() else {
            state = .disconnected
            scheduleReconnect()
            return
        }

        // Negotiate (HTTP POST). Получаем connectionToken для WS.
        let negotiate: NegotiateResponse
        do {
            negotiate = try await postNegotiate(token: token)
        } catch {
            state = .disconnected
            scheduleReconnect()
            return
        }

        // Сборка WS-URL: ?id=<connectionToken>&access_token=<jwt>
        guard var components = URLComponents(url: hubURL, resolvingAgainstBaseURL: false) else {
            state = .disconnected
            scheduleReconnect()
            return
        }
        components.scheme = (components.scheme == "https") ? "wss" : "ws"
        var query = components.queryItems ?? []
        query.append(URLQueryItem(name: "id", value: negotiate.connectionToken))
        query.append(URLQueryItem(name: "access_token", value: token))
        components.queryItems = query

        guard let wsURL = components.url else {
            state = .disconnected
            scheduleReconnect()
            return
        }

        let req = URLRequest(url: wsURL)
        let ws = session.webSocketTask(with: req)
        self.task = ws
        ws.resume()

        // Handshake — JSON-протокол, версия 1.
        let handshake: [String: Any] = ["protocol": "json", "version": 1]
        guard let handshakeData = try? JSONSerialization.data(withJSONObject: handshake) else {
            await tearDown()
            scheduleReconnect()
            return
        }
        await send(frame: handshakeData)

        // Запускаем приём.
        receiveTask = Task { [weak self] in
            await self?.receiveLoop()
        }

        keepaliveTask = Task { [weak self] in
            await self?.keepaliveLoop()
        }
    }

    private func postNegotiate(token: String) async throws -> NegotiateResponse {
        var components = URLComponents(url: hubURL.appendingPathComponent("negotiate"), resolvingAgainstBaseURL: false)!
        var query = components.queryItems ?? []
        query.append(URLQueryItem(name: "negotiateVersion", value: "1"))
        components.queryItems = query

        var req = URLRequest(url: components.url!)
        req.httpMethod = "POST"
        req.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.httpBody = Data()

        let (data, response) = try await session.data(for: req)
        guard let http = response as? HTTPURLResponse, (200..<300).contains(http.statusCode) else {
            throw URLError(.userAuthenticationRequired)
        }
        return try JSONDecoder().decode(NegotiateResponse.self, from: data)
    }

    // MARK: - Send / receive

    private func send(frame data: Data) async {
        guard let task else { return }
        var payload = data
        payload.append(Self.recordSeparator)
        guard let str = String(data: payload, encoding: .utf8) else { return }
        do {
            try await task.send(.string(str))
        } catch {
            await handleDisconnect()
        }
    }

    private func receiveLoop() async {
        guard let task else { return }
        while !Task.isCancelled {
            do {
                let message = try await task.receive()
                switch message {
                case .string(let text):
                    await handle(text: text)
                case .data(let data):
                    if let text = String(data: data, encoding: .utf8) {
                        await handle(text: text)
                    }
                @unknown default:
                    break
                }
            } catch {
                await handleDisconnect()
                return
            }
        }
    }

    private func handle(text: String) async {
        // Сообщения отделены 0x1E. Первое сообщение от сервера — ответ на
        // handshake: `{}<RS>`. Дальше пошли invocation'ы.
        for piece in text.split(separator: Character(UnicodeScalar(Self.recordSeparator))) {
            let s = String(piece)
            guard !s.isEmpty, let data = s.data(using: .utf8) else { continue }
            guard let obj = try? JSONSerialization.jsonObject(with: data) as? [String: Any] else { continue }

            // Handshake response — пустой объект (или {"error":"..."}).
            if state == .connecting {
                if obj["error"] == nil {
                    state = .connected
                    for cb in onConnected {
                        await cb()
                    }
                } else {
                    await tearDown()
                    scheduleReconnect()
                    return
                }
                continue
            }

            let type = obj["type"] as? Int ?? 0
            switch type {
            case 1:
                // Invocation.
                guard let target = obj["target"] as? String else { continue }
                let argsAny = obj["arguments"] as? [Any] ?? []
                let argsData = (try? JSONSerialization.data(withJSONObject: argsAny)) ?? Data()
                if let handlers = handlers[target] {
                    for h in handlers {
                        await h(argsData)
                    }
                }
            case 6:
                // KeepAlive — игнорируем (клиент сам шлёт пинг по таймеру).
                break
            case 7:
                // Close — сервер инициировал разрыв.
                await tearDown()
                scheduleReconnect()
                return
            default:
                break
            }
        }
    }

    private func keepaliveLoop() async {
        // Спецификация SignalR: клиент шлёт ping не реже 30с, иначе сервер закрывает.
        while !Task.isCancelled, state == .connected || state == .connecting {
            try? await Task.sleep(nanoseconds: 15_000_000_000)
            guard state == .connected else { continue }
            let ping: [String: Any] = ["type": 6]
            if let data = try? JSONSerialization.data(withJSONObject: ping) {
                await send(frame: data)
            }
        }
    }

    private func handleDisconnect() async {
        await tearDown()
        scheduleReconnect()
    }

    private func tearDown() async {
        receiveTask?.cancel(); receiveTask = nil
        keepaliveTask?.cancel(); keepaliveTask = nil
        task?.cancel(with: .goingAway, reason: nil)
        task = nil
        state = .disconnected
    }

    private func scheduleReconnect() {
        guard reconnectTask == nil else { return }
        state = .reconnecting
        reconnectTask = Task { [weak self] in
            try? await Task.sleep(nanoseconds: 3_000_000_000)
            await self?.clearReconnectTask()
            await self?.start()
        }
    }

    private func clearReconnectTask() {
        reconnectTask = nil
    }
}

// MARK: - Negotiate response

private struct NegotiateResponse: Decodable {
    let connectionToken: String
    let connectionId: String?
}
