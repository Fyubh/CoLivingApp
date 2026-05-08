import Foundation
import Observation

/// Single source of truth for the auth flow. Three legal states — `signedOut`,
/// `requiresPasswordChange`, `signedIn` — drive the root `AuthFlow` switch
/// directly. Token lives in Keychain; `state` only carries the in-memory
/// reference so callers don't keep round-tripping through the keychain.
///
/// Bootstrap reads the keychain on init and trusts an existing token (no
/// expiry check yet — `/Auth/my-context` will validate when Phase 3 lands).
/// If the token has expired, the next authed call returns `.unauthorized`
/// and the caller can `signOut()` to reset.
@Observable
final class AuthStore {
    enum State: Equatable {
        case signedOut
        case requiresPasswordChange(token: String)
        case signedIn(token: String)
    }

    private(set) var state: State = .signedOut
    private(set) var isLoading: Bool = false
    var error: String? = nil

    private let api: APIClient

    init(api: APIClient = .shared) {
        self.api = api
        bootstrap()
    }

    private func bootstrap() {
        if let token = Keychain.load() {
            state = .signedIn(token: token)
        }
    }

    func signIn(email: String, password: String) async {
        isLoading = true
        error = nil
        defer { isLoading = false }

        do {
            let resp = try await api.login(email: email, password: password)
            Keychain.save(resp.token)
            state = resp.mustChangePassword
                ? .requiresPasswordChange(token: resp.token)
                : .signedIn(token: resp.token)
        } catch {
            self.error = friendlyMessage(for: error, fallback: "Не удалось войти. Проверьте данные.")
        }
    }

    func changePassword(oldPassword: String, newPassword: String) async {
        guard case .requiresPasswordChange(let token) = state else { return }

        isLoading = true
        error = nil
        defer { isLoading = false }

        do {
            let resp = try await api.changePassword(
                oldPassword: oldPassword,
                newPassword: newPassword,
                token: token
            )
            Keychain.save(resp.token)
            state = .signedIn(token: resp.token)
        } catch {
            self.error = friendlyMessage(for: error, fallback: "Не удалось сохранить пароль.")
        }
    }

    func signOut() {
        Keychain.clear()
        state = .signedOut
        error = nil
    }

    private func friendlyMessage(for error: Error, fallback: String) -> String {
        if let api = error as? APIError {
            switch api {
            case .server(let msg):
                return msg
            case .unauthorized:
                return "Неверный e-mail или пароль."
            case .network:
                return "Нет соединения с сервером."
            default:
                return api.errorDescription ?? fallback
            }
        }
        return error.localizedDescription.isEmpty ? fallback : error.localizedDescription
    }
}
