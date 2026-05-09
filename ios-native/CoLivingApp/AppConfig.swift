import Foundation

/// App-wide constants kept out of source-controlled secrets. Values match
/// the RN reference (`resident-app/src/config.ts`) so support routing stays
/// consistent during the cutover. Pre-TestFlight todo: read overrides from
/// `Info.plist` so staging/prod can ship different endpoints.
enum AppConfig {
    /// Recipient for support-mailto links and account-deletion requests.
    static let supportEmail = "support@coliving-os.example"

    /// Public privacy policy. Opened in Safari from the Profile tab.
    static let privacyPolicyURL = URL(string: "https://example.com/privacy")!
}
