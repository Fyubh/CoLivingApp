import Foundation

// MARK: - Enums (read=int via explicit cast, write=string via JsonStringEnumConverter)

/// Mirrors `CoLivingApp.Domain.Enums.ItemStatus`. MVP only renders and
/// creates `available` items; the InCart / Consumed states exist on the
/// backend but the resident UI doesn't expose them yet (no checkout flow,
/// so the round-trip Cart → Available is impossible without cross-module
/// coupling to Expenses).
nonisolated enum ItemStatus: String, Codable, CaseIterable, Identifiable {
    case available = "Available"
    case runningLow = "RunningLow"
    case inCart = "InCart"
    case consumed = "Consumed"

    var id: String { rawValue }

    init?(categoryId: Int) {
        let all = ItemStatus.allCases
        guard categoryId >= 0 && categoryId < all.count else { return nil }
        self = all[categoryId]
    }
}

nonisolated enum ItemCategory: String, Codable, CaseIterable, Identifiable {
    case food = "Food"
    case household = "Household"
    case hygiene = "Hygiene"
    case other = "Other"

    var id: String { rawValue }

    init?(categoryId: Int) {
        let all = ItemCategory.allCases
        guard categoryId >= 0 && categoryId < all.count else { return nil }
        self = all[categoryId]
    }

    var label: String {
        switch self {
        case .food:      return "Еда"
        case .household: return "Хозтовары"
        case .hygiene:   return "Гигиена"
        case .other:     return "Другое"
        }
    }

    var icon: String {
        switch self {
        case .food:      return "fork.knife"
        case .household: return "shippingbox"
        case .hygiene:   return "bubbles.and.sparkles"
        case .other:     return "ellipsis"
        }
    }
}

nonisolated enum UnitType: String, Codable, CaseIterable, Identifiable {
    case piece = "Piece"
    case package = "Package"
    case bottle = "Bottle"
    case kilogram = "Kilogram"
    case gram = "Gram"
    case liter = "Liter"
    case milliliter = "Milliliter"

    var id: String { rawValue }

    init?(categoryId: Int) {
        let all = UnitType.allCases
        guard categoryId >= 0 && categoryId < all.count else { return nil }
        self = all[categoryId]
    }

    var shortLabel: String {
        switch self {
        case .piece:      return "шт"
        case .package:    return "уп"
        case .bottle:     return "бут"
        case .kilogram:   return "кг"
        case .gram:       return "г"
        case .liter:      return "л"
        case .milliliter: return "мл"
        }
    }
}

nonisolated enum StorageLocation: String, Codable, CaseIterable, Identifiable {
    case fridge = "Fridge"
    case freezer = "Freezer"
    case pantry = "Pantry"
    case bathroom = "Bathroom"
    case other = "Other"

    var id: String { rawValue }

    init?(categoryId: Int) {
        let all = StorageLocation.allCases
        guard categoryId >= 0 && categoryId < all.count else { return nil }
        self = all[categoryId]
    }

    var label: String {
        switch self {
        case .fridge:   return "Холодильник"
        case .freezer:  return "Морозилка"
        case .pantry:   return "Кухонный шкаф"
        case .bathroom: return "Ванная"
        case .other:    return "Другое"
        }
    }

    var icon: String {
        switch self {
        case .fridge:   return "refrigerator"
        case .freezer:  return "snowflake"
        case .pantry:   return "cabinet"
        case .bathroom: return "shower"
        case .other:    return "tray"
        }
    }
}

// MARK: - Read DTO

/// Mirrors `ItemDto` from
/// `Application/Features/Inventory/Queries/GetItems`. Enum fields come over
/// the wire as ints (handler casts `(int)i.Unit` etc.); we map them to
/// typed enums via `init?(categoryId:)` for display, but keep the raw int
/// available for tolerance to unknown values.
nonisolated struct ItemDto: Decodable, Identifiable {
    let id: UUID
    let name: String
    let quantity: Decimal
    let unit: Int
    let category: Int
    let location: Int
    let expiryDate: Date?

    var unitType: UnitType? { UnitType(categoryId: unit) }
    var itemCategory: ItemCategory? { ItemCategory(categoryId: category) }
    var storageLocation: StorageLocation? { StorageLocation(categoryId: location) }
}

// MARK: - Write payloads

/// Mirrors `CreateItemCommand`. `userId` is rewritten by the controller
/// from the JWT. MVP always sends `Status=Available` — the only bucket the
/// resident UI exposes at the moment.
nonisolated struct CreateItemPayload: Encodable {
    let apartmentId: UUID
    let customName: String?
    let quantity: Decimal
    let unit: UnitType
    let status: ItemStatus
    let userId: String
    let category: ItemCategory
    let location: StorageLocation
    let expiryDate: Date?

    init(
        apartmentId: UUID,
        customName: String?,
        quantity: Decimal,
        unit: UnitType,
        category: ItemCategory,
        location: StorageLocation,
        expiryDate: Date? = nil
    ) {
        self.apartmentId = apartmentId
        self.customName = customName
        self.quantity = quantity
        self.unit = unit
        self.status = .available
        self.userId = ""
        self.category = category
        self.location = location
        self.expiryDate = expiryDate
    }
}

/// Mirrors `ConsumeItemCommand`. `userId` is rewritten by the controller.
nonisolated struct ConsumeItemPayload: Encodable {
    let itemId: UUID
    let apartmentId: UUID
    let userId: String

    init(itemId: UUID, apartmentId: UUID) {
        self.itemId = itemId
        self.apartmentId = apartmentId
        self.userId = ""
    }
}

// MARK: - Response envelope

nonisolated struct CreateItemResponse: Decodable {
    let itemId: UUID
}
