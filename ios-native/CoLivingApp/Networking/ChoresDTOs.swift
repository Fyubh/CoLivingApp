import Foundation

// MARK: - Enums

/// Mirrors `CoLivingApp.Domain.Enums.ChoreCategory`. Both read DTO and
/// write payload use raw `int` here (the create command takes `int Category`
/// directly, not the typed enum) — easier than the read-int / write-string
/// split we have for Expenses and Inventory.
nonisolated enum ChoreCategory: Int, Codable, CaseIterable, Identifiable {
    case vacuum = 0
    case mop = 1
    case toilet = 2
    case kitchen = 3
    case dishes = 4
    case trash = 5
    case other = 6

    var id: Int { rawValue }

    var label: String {
        switch self {
        case .vacuum:  return "Пылесос"
        case .mop:     return "Мытьё полов"
        case .toilet:  return "Туалет / ванная"
        case .kitchen: return "Кухня"
        case .dishes:  return "Посуда"
        case .trash:   return "Мусор"
        case .other:   return "Другое"
        }
    }

    var icon: String {
        switch self {
        case .vacuum:  return "wind"
        case .mop:     return "drop"
        case .toilet:  return "shower"
        case .kitchen: return "fork.knife"
        case .dishes:  return "cup.and.saucer"
        case .trash:   return "trash"
        case .other:   return "ellipsis"
        }
    }
}

/// Mirrors `CoLivingApp.Domain.Enums.ChoreStatus`. Workflow:
///   `pending` → `needsReview` (assignee marks complete)
///   `needsReview` → `completed` (someone else confirms)
///   `needsReview` → `pending` (someone else rejects, +€5 penalty expense)
nonisolated enum ChoreStatus: Int, Codable, CaseIterable {
    case pending = 0
    case needsReview = 1
    case completed = 2

    var label: String {
        switch self {
        case .pending:     return "К выполнению"
        case .needsReview: return "На проверке"
        case .completed:   return "Сделано"
        }
    }
}

// MARK: - Read DTO

/// Mirrors `ChoreDto`. `canComplete` / `canReview` are server-computed:
///   * `canComplete` = status==Pending && (assignee is unset OR == me)
///   * `canReview`   = status==NeedsReview && assignee != me
/// We trust them — the same rules are enforced in the workflow handlers.
nonisolated struct ChoreDto: Decodable, Identifiable {
    let id: UUID
    let title: String
    let description: String?
    let category: Int
    let status: Int
    let dueDate: Date?
    let assignedName: String?
    let canComplete: Bool
    let canReview: Bool

    var choreCategory: ChoreCategory? { ChoreCategory(rawValue: category) }
    var choreStatus: ChoreStatus { ChoreStatus(rawValue: status) ?? .pending }
}

// MARK: - Write payloads

/// Mirrors `CreateChoreCommand`. `assignedUserId` is optional — when nil
/// the chore is "anyone can take it" and any roommate can complete it.
nonisolated struct CreateChorePayload: Encodable {
    let apartmentId: UUID
    let title: String
    let description: String?
    let category: Int
    let assignedUserId: String?
    let dueDate: Date?

    init(
        apartmentId: UUID,
        title: String,
        description: String?,
        category: ChoreCategory,
        assignedUserId: String?,
        dueDate: Date?
    ) {
        self.apartmentId = apartmentId
        self.title = title
        self.description = description
        self.category = category.rawValue
        self.assignedUserId = assignedUserId
        self.dueDate = dueDate
    }
}

/// Body for `POST /Chores/{id}/{complete|confirm|reject}`. Server-side, the
/// controller overrides `ChoreId` from the URL and `UserId` from the JWT,
/// so we send empty values and let it overwrite.
nonisolated struct ChoreActionPayload: Encodable {
    let choreId: UUID
    let apartmentId: UUID
    let userId: String

    init(apartmentId: UUID) {
        self.choreId = UUID(uuid: (0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0))
        self.apartmentId = apartmentId
        self.userId = ""
    }
}

// MARK: - Response envelope

nonisolated struct CreateChoreResponse: Decodable {
    let choreId: UUID
}
