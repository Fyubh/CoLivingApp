import Foundation

/// Mirrors `MeDto` from
/// `CoLivingApp.Application/Features/Users/Queries/GetMe`.
/// Returned by `GET /api/Auth/me`. Only `Profile` consumes this for now —
/// other screens get the user's name from `MyApartmentContextDto.meName`.
nonisolated struct MeDto: Decodable {
    let id: String
    let email: String
    let name: String
    /// Backend serializes the role enum as a string ("Tenant", "Staff",
    /// "Admin", "SuperAdmin"). Kept as raw string here — the screen maps
    /// it to a localized label via `MeDto.roleLabel(_:)`.
    let role: String
    let accessLevel: Int

    /// Russian label for the resident-app pill. Unknown roles fall back
    /// to the raw string so we surface, not silently drop, new values.
    var roleLabel: String {
        switch role {
        case "Tenant":     return "Жилец"
        case "Staff":      return "Персонал"
        case "Admin":      return "Администрация"
        case "SuperAdmin": return "Управление"
        default:           return role
        }
    }
}
