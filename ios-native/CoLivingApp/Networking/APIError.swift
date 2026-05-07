import Foundation

/// Structured network/HTTP errors. `.server(message:)` carries the
/// `{ "error": "..." }` envelope from the backend so call sites can show the
/// localized backend message verbatim instead of inventing their own copy.
enum APIError: Error, LocalizedError {
    case invalidURL
    case network(URLError)
    case http(status: Int, body: String?)
    case server(message: String)
    case unauthorized
    case decode(Error)

    var errorDescription: String? {
        switch self {
        case .invalidURL:
            return "Невалидный адрес запроса."
        case .network:
            return "Нет соединения с сервером."
        case .http(let status, _):
            return "Ошибка сервера (\(status))."
        case .server(let message):
            return message
        case .unauthorized:
            return "Сессия истекла. Войдите заново."
        case .decode:
            return "Не удалось разобрать ответ сервера."
        }
    }
}
