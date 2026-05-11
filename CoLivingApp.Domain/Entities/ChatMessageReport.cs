namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Жалоба на сообщение в чате квартиры. Создаётся любым жильцом, кроме автора
/// сообщения. App Review требует возможность report для любой UGC-поверхности,
/// плюс предотвращение спама — поэтому на (MessageId, ReporterId) висит unique
/// index (см. ChatMessageReportConfiguration).
/// </summary>
public class ChatMessageReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatMessage? Message { get; set; }
    public User? Reporter { get; set; }
}
