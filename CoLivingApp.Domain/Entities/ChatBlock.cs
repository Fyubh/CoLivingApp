namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Блокировка одного жильца другим. Скрывает сообщения BlockedUserId из
/// GetChatHistoryQuery со стороны Blocker'а. Симметричной блокировки нет —
/// заблокированный жилец продолжает видеть сообщения Blocker'а (так же как
/// в большинстве мессенджеров, и так же требует App Review).
///
/// Уникальность по (BlockerId, BlockedUserId) — см. ChatBlockConfiguration.
/// Идемпотентность блокировки на уровне команды.
/// </summary>
public class ChatBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BlockerId { get; set; } = string.Empty;
    public string BlockedUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Blocker { get; set; }
    public User? BlockedUser { get; set; }
}
