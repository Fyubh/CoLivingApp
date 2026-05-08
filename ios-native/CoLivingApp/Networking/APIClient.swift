import Foundation

/// Thin actor over `URLSession`. Endpoints are hand-rolled — no codegen, no
/// SPM client. The generic `post` keeps boilerplate at the call site to
/// `path` + body + token.
///
/// Backend is ASP.NET Core 9 with default camelCase JSON, so Swift `Codable`
/// PascalCase doesn't apply — properties match camelCase as written.
actor APIClient {
    static let shared = APIClient()

    private let baseURL: URL
    private let session: URLSession
    private let decoder: JSONDecoder
    private let encoder: JSONEncoder

    init(baseURL: URL = URL(string: "http://localhost:5130/api/")!) {
        self.baseURL = baseURL
        self.session = .shared
        self.decoder = JSONDecoder()
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

    // MARK: - Generic

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

        // Backend convention: non-2xx with `{ "error": "..." }` envelope.
        if let envelope = try? decoder.decode(ErrorEnvelope.self, from: data) {
            throw APIError.server(message: envelope.error)
        }
        if http.statusCode == 401 {
            throw APIError.unauthorized
        }
        throw APIError.http(status: http.statusCode, body: String(data: data, encoding: .utf8))
    }
}

// MARK: - DTOs

nonisolated struct AuthTokenResponse: Decodable {
    let token: String
    let mustChangePassword: Bool
}

nonisolated private struct LoginRequest: Encodable {
    let email: String
    let password: String
}

nonisolated private struct ChangePasswordRequest: Encodable {
    let oldPassword: String
    let newPassword: String
}

nonisolated private struct ErrorEnvelope: Decodable {
    let error: String
}
