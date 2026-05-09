import Foundation

// MARK: - Category

/// Mirrors `CoLivingApp.Domain.Enums.ExpenseCategory`. Backend reads/writes
/// this in two shapes inconsistent with each other:
///   * `GET /Expenses/{apt}` returns `categoryId: int` (handler casts the
///     enum to int explicitly).
///   * `POST /Expenses` accepts `category: ExpenseCategory` typed as the
///     enum, which `JsonStringEnumConverter` serializes as a string.
///
/// We expose a `String, Codable` enum (for encoding writes) and a
/// `init?(categoryId:)` for decoding read DTOs. Order MUST match the C#
/// enum declaration since the int we receive is the ordinal.
nonisolated enum ExpenseCategory: String, Codable, CaseIterable, Identifiable {
    case groceries = "Groceries"
    case rent = "Rent"
    case utilities = "Utilities"
    case internet = "Internet"
    case household = "Household"
    case other = "Other"

    var id: String { rawValue }

    init?(categoryId: Int) {
        let all = ExpenseCategory.allCases
        guard categoryId >= 0 && categoryId < all.count else { return nil }
        self = all[categoryId]
    }

    var label: String {
        switch self {
        case .groceries: return "Продукты"
        case .rent:      return "Аренда"
        case .utilities: return "Коммуналка"
        case .internet:  return "Интернет"
        case .household: return "Хозтовары"
        case .other:     return "Другое"
        }
    }

    var icon: String {
        switch self {
        case .groceries: return "cart"
        case .rent:      return "house"
        case .utilities: return "lightbulb"
        case .internet:  return "wifi"
        case .household: return "shippingbox"
        case .other:     return "ellipsis"
        }
    }
}

// MARK: - Read DTOs

/// Mirrors `ExpenseDto` from
/// `CoLivingApp.Application/Features/Expenses/Queries/GetExpenses`.
nonisolated struct ExpenseDto: Decodable, Identifiable {
    let id: UUID
    let description: String
    let amount: Decimal
    let date: Date
    let payerId: String
    let payerName: String
    let categoryId: Int

    var category: ExpenseCategory? { ExpenseCategory(categoryId: categoryId) }
}

/// Mirrors `UserBalanceDto`. Sign convention: positive = roommate owes me;
/// negative = I owe roommate. The handler filters out the current user, so
/// only the *other* roommates appear here.
nonisolated struct UserBalanceDto: Decodable, Identifiable {
    let userId: String
    let userName: String
    let balance: Decimal

    var id: String { userId }
}

// MARK: - Write payloads

/// Mirrors `CreateExpenseCommand`. `payerId` is rewritten by the controller
/// from the JWT before the handler runs — we ship `""` and the server
/// overwrites it.
nonisolated struct CreateExpensePayload: Encodable {
    let apartmentId: UUID
    let payerId: String
    let amount: Decimal
    let description: String
    let category: ExpenseCategory

    init(apartmentId: UUID, amount: Decimal, description: String, category: ExpenseCategory) {
        self.apartmentId = apartmentId
        self.payerId = ""
        self.amount = amount
        self.description = description
        self.category = category
    }
}

/// Mirrors `SettleDebtCommand`. Sender is the current user; the server
/// rewrites `senderId` from the JWT. Only meaningful when the current user
/// owes someone (i.e. `Balance < 0` for that roommate).
nonisolated struct SettleDebtPayload: Encodable {
    let apartmentId: UUID
    let receiverId: String
    let amount: Decimal
    let senderId: String

    init(apartmentId: UUID, receiverId: String, amount: Decimal) {
        self.apartmentId = apartmentId
        self.receiverId = receiverId
        self.amount = amount
        self.senderId = ""
    }
}

// MARK: - Response envelopes

nonisolated struct CreateExpenseResponse: Decodable {
    let expenseId: UUID
}

nonisolated struct SettleDebtResponse: Decodable {
    let settlementId: UUID
}
