import Foundation

/// Mirrors `MyApartmentContextDto` from
/// `CoLivingApp.Application/Features/Apartments/Queries/GetMyApartmentContext`.
/// Property names match the backend's camelCase JSON.
nonisolated struct MyApartmentContextDto: Decodable {
    let apartmentId: UUID
    let apartmentName: String
    let unitNumber: String?
    let buildingId: UUID?
    let buildingName: String?
    let rooms: [RoomOptionDto]
    let meUserId: String
    let meName: String
    let roommates: [RoommateDto]
}

nonisolated struct RoomOptionDto: Decodable, Hashable {
    let id: UUID
    let number: String
    let typeLabel: String
}

nonisolated struct RoommateDto: Decodable, Identifiable {
    var id: String { userId }
    let userId: String
    let name: String
    let roomNumber: String?
    let joinedAt: Date
    let isMe: Bool
}

/// Mirrors `ResidentNotificationDto`. The `audience` field exists on the
/// backend (`NotificationAudience` string enum) but Phase 3 doesn't render
/// it differently per audience, so it's intentionally omitted — `Decodable`
/// drops unknown fields silently.
nonisolated struct ResidentNotificationDto: Decodable, Identifiable, Equatable {
    let id: UUID
    let buildingId: UUID
    let title: String
    let body: String
    let isImportant: Bool
    let isRead: Bool
    let createdAt: Date
    let readAt: Date?
}
