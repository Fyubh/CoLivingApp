import Foundation

/// Thin actor over `URLSession`. Endpoints are hand-rolled — no codegen, no
/// SPM client. Generic `get` / `post` build the request; `send` runs it and
/// maps status / envelope into `APIError`. Backend is ASP.NET Core 9 with
/// default camelCase JSON, `JsonStringEnumConverter` registered, ISO 8601
/// dates (with or without fractional seconds depending on column precision).
actor APIClient {
    static let shared = APIClient()

    private let baseURL: URL
    private let session: URLSession
    private let decoder: JSONDecoder
    private let encoder: JSONEncoder

    init(baseURL: URL = URL(string: "http://localhost:5130/api/")!) {
        self.baseURL = baseURL
        self.session = .shared

        let dec = JSONDecoder()
        // Backend sometimes returns `2026-05-08T14:51:56.123Z`, sometimes
        // `2026-05-08T14:51:56Z` — try both and fail loudly otherwise. A new
        // formatter per decode is fine: the cost is microseconds and there's
        // no shared mutable state to worry about.
        dec.dateDecodingStrategy = .custom { decoder in
            let str = try decoder.singleValueContainer().decode(String.self)
            let f = ISO8601DateFormatter()
            f.formatOptions = [.withInternetDateTime, .withFractionalSeconds]
            if let d = f.date(from: str) { return d }
            f.formatOptions = [.withInternetDateTime]
            if let d = f.date(from: str) { return d }
            throw DecodingError.dataCorrupted(
                .init(codingPath: decoder.codingPath,
                      debugDescription: "Invalid ISO 8601 date: \(str)")
            )
        }
        self.decoder = dec
        self.encoder = JSONEncoder()
    }

    // MARK: - Auth endpoints

    func login(email: String, password: String) async throws -> AuthTokenResponse {
        let body = LoginRequest(email: email, password: password)
        return try await post(path: "Auth/login", body: body, token: nil)
    }

    func changePassword(
        oldPassword: String,
        newPassword: String,
        token: String
    ) async throws -> AuthTokenResponse {
        let body = ChangePasswordRequest(oldPassword: oldPassword, newPassword: newPassword)
        return try await post(path: "Users/change-password", body: body, token: token)
    }

    // MARK: - User endpoints

    /// Used by Profile to render identity (avatar / name / email / role).
    /// Cheap query — no joins server-side, just the Users row.
    func getMe(token: String) async throws -> MeDto {
        return try await get(path: "Auth/me", token: token)
    }

    // MARK: - Home endpoints

    /// Returns `nil` when the user has no active apartment membership — the
    /// backend serializes a missing membership as `200 OK` with body `null`,
    /// not 404. Optional<T> + JSONDecoder handle the literal `null` natively.
    func getMyApartmentContext(token: String) async throws -> MyApartmentContextDto? {
        return try await get(path: "Apartments/my-context", token: token)
    }

    func getMyNotifications(token: String) async throws -> [ResidentNotificationDto] {
        return try await get(path: "Notifications/my", token: token)
    }

    func markNotificationRead(id: UUID, token: String) async throws {
        let _: MarkReadResponse = try await post(
            path: "Notifications/\(id.uuidString)/read",
            body: EmptyBody(),
            token: token
        )
    }

    // MARK: - Maintenance endpoints

    func getMyMaintenanceRequests(token: String) async throws -> [MaintenanceRequestDto] {
        return try await get(path: "Maintenance/my", token: token)
    }

    func createMaintenanceRequest(
        payload: CreateMaintenancePayload,
        token: String
    ) async throws -> UUID {
        let resp: CreateMaintenanceResponse = try await post(
            path: "Maintenance",
            body: payload,
            token: token
        )
        return resp.maintenanceRequestId
    }

    func cancelMaintenanceRequest(
        id: UUID,
        reason: String?,
        token: String
    ) async throws {
        let _: MaintenanceWorkflowResponse = try await post(
            path: "Maintenance/\(id.uuidString)/cancel",
            body: CancelMaintenancePayload(reason: reason),
            token: token
        )
    }

    func rateMaintenanceRequest(
        id: UUID,
        rating: Int,
        feedback: String?,
        token: String
    ) async throws {
        let _: MaintenanceWorkflowResponse = try await post(
            path: "Maintenance/\(id.uuidString)/rate",
            body: RateMaintenancePayload(rating: rating, feedback: feedback),
            token: token
        )
    }

    // MARK: - Finance (Expenses) endpoints

    func getExpenses(apartmentId: UUID, token: String) async throws -> [ExpenseDto] {
        return try await get(path: "Expenses/\(apartmentId.uuidString)", token: token)
    }

    func getBalance(apartmentId: UUID, token: String) async throws -> [UserBalanceDto] {
        return try await get(path: "Expenses/balance/\(apartmentId.uuidString)", token: token)
    }

    func createExpense(payload: CreateExpensePayload, token: String) async throws -> UUID {
        let resp: CreateExpenseResponse = try await post(
            path: "Expenses",
            body: payload,
            token: token
        )
        return resp.expenseId
    }

    func settleDebt(payload: SettleDebtPayload, token: String) async throws -> UUID {
        let resp: SettleDebtResponse = try await post(
            path: "Expenses/settle",
            body: payload,
            token: token
        )
        return resp.settlementId
    }

    // MARK: - Inventory endpoints

    func getInventoryItems(
        apartmentId: UUID,
        status: ItemStatus,
        token: String
    ) async throws -> [ItemDto] {
        return try await get(
            path: "Inventory/\(apartmentId.uuidString)",
            token: token,
            query: [URLQueryItem(name: "status", value: status.rawValue)]
        )
    }

    func createInventoryItem(payload: CreateItemPayload, token: String) async throws -> UUID {
        let resp: CreateItemResponse = try await post(
            path: "Inventory",
            body: payload,
            token: token
        )
        return resp.itemId
    }

    /// Soft-delete via status: pivots Available → Consumed. Server returns
    /// `200 OK` with an empty body, so we route through `postIgnoringBody`.
    func consumeInventoryItem(itemId: UUID, apartmentId: UUID, token: String) async throws {
        try await postIgnoringBody(
            path: "Inventory/consume",
            body: ConsumeItemPayload(itemId: itemId, apartmentId: apartmentId),
            token: token
        )
    }

    /// Hard delete. Both ids live in the URL; no body. Server returns
    /// `200 OK` with an empty body.
    func removeInventoryItem(itemId: UUID, apartmentId: UUID, token: String) async throws {
        try await deleteRequest(
            path: "Inventory/\(itemId.uuidString)/\(apartmentId.uuidString)",
            token: token
        )
    }

    // MARK: - Generic

    private func get<Resp: Decodable>(
        path: String,
        token: String?,
        query: [URLQueryItem] = []
    ) async throws -> Resp {
        guard
            let initial = URL(string: path, relativeTo: baseURL),
            var components = URLComponents(url: initial, resolvingAgainstBaseURL: true)
        else {
            throw APIError.invalidURL
        }
        if !query.isEmpty {
            components.queryItems = query
        }
        guard let url = components.url else { throw APIError.invalidURL }

        var req = URLRequest(url: url)
        req.httpMethod = "GET"
        req.setValue("application/json", forHTTPHeaderField: "Accept")
        if let token {
            req.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        return try await send(req)
    }

    private func post<Body: Encodable, Resp: Decodable>(
        path: String,
        body: Body,
        token: String?
    ) async throws -> Resp {
        guard let url = URL(string: path, relativeTo: baseURL) else {
            throw APIError.invalidURL
        }
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.setValue("application/json", forHTTPHeaderField: "Accept")
        if let token {
            req.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        req.httpBody = try encoder.encode(body)
        return try await send(req)
    }

    /// POST with a body whose response is `200 OK` with no body. Used by
    /// workflow endpoints (Inventory consume, Chores complete/confirm/reject)
    /// that don't return anything meaningful — saves us inventing throwaway
    /// `WorkflowResponse` envelopes per call site.
    private func postIgnoringBody<Body: Encodable>(
        path: String,
        body: Body,
        token: String?
    ) async throws {
        guard let url = URL(string: path, relativeTo: baseURL) else {
            throw APIError.invalidURL
        }
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.setValue("application/json", forHTTPHeaderField: "Accept")
        if let token {
            req.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        req.httpBody = try encoder.encode(body)
        try await sendIgnoringBody(req)
    }

    /// Bodyless DELETE. Server returns `200 OK` with no body.
    private func deleteRequest(path: String, token: String?) async throws {
        guard let url = URL(string: path, relativeTo: baseURL) else {
            throw APIError.invalidURL
        }
        var req = URLRequest(url: url)
        req.httpMethod = "DELETE"
        if let token {
            req.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        try await sendIgnoringBody(req)
    }

    private func sendIgnoringBody(_ req: URLRequest) async throws {
        let data: Data
        let response: URLResponse
        do {
            (data, response) = try await session.data(for: req)
        } catch let urlError as URLError {
            throw APIError.network(urlError)
        }

        guard let http = response as? HTTPURLResponse else {
            throw APIError.http(status: -1, body: nil)
        }

        if (200..<300).contains(http.statusCode) { return }

        if let envelope = try? decoder.decode(ErrorEnvelope.self, from: data) {
            throw APIError.server(message: envelope.error)
        }
        if http.statusCode == 401 {
            throw APIError.unauthorized
        }
        throw APIError.http(status: http.statusCode, body: String(data: data, encoding: .utf8))
    }

    private func send<Resp: Decodable>(_ req: URLRequest) async throws -> Resp {
        let data: Data
        let response: URLResponse
        do {
            (data, response) = try await session.data(for: req)
        } catch let urlError as URLError {
            throw APIError.network(urlError)
        }

        guard let http = response as? HTTPURLResponse else {
            throw APIError.http(status: -1, body: nil)
        }

        if (200..<300).contains(http.statusCode) {
            do {
                return try decoder.decode(Resp.self, from: data)
            } catch {
                throw APIError.decode(error)
            }
        }

        if let envelope = try? decoder.decode(ErrorEnvelope.self, from: data) {
            throw APIError.server(message: envelope.error)
        }
        if http.statusCode == 401 {
            throw APIError.unauthorized
        }
        throw APIError.http(status: http.statusCode, body: String(data: data, encoding: .utf8))
    }
}

// MARK: - DTOs (auth + plumbing)

nonisolated struct AuthTokenResponse: Decodable {
    let token: String
    let mustChangePassword: Bool
}

nonisolated struct MarkReadResponse: Decodable {
    let notificationId: UUID
}

nonisolated private struct LoginRequest: Encodable {
    let email: String
    let password: String
}

nonisolated private struct ChangePasswordRequest: Encodable {
    let oldPassword: String
    let newPassword: String
}

nonisolated private struct EmptyBody: Encodable {}

nonisolated private struct ErrorEnvelope: Decodable {
    let error: String
}
