import Foundation

/// Mirrors `ChatMessageDto` from
/// `CoLivingApp.Application/Features/Chat/Queries/GetChatHistoryQuery.cs`.
///
/// `isDeleted=true` означает: автор удалил своё сообщение. Сервер на этот
/// случай уже стёр `text` (пустая строка) — iOS подставляет
/// «[Сообщение удалено]» в UI.
nonisolated struct ChatMessageDto: Decodable, Identifiable, Equatable {
    let id: UUID
    let senderId: String
    let senderName: String
    let text: String
    let sentAt: Date
    let isDeleted: Bool
}

// MARK: - Write payloads

nonisolated struct SendMessagePayload: Encodable {
    let apartmentId: UUID
    let text: String
}

/// Тело `POST /Chat/messages/{id}/report`. Reason опциональный — App Review
/// требует возможность пожаловаться без обязательного комментария.
nonisolated struct ReportMessagePayload: Encodable {
    let reason: String?
}

nonisolated struct BlockUserPayload: Encodable {
    let blockedUserId: String
}
