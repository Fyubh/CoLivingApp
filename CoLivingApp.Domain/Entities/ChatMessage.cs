namespace CoLivingApp.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete: ставится автором через DeleteMessageCommand. Сообщение
    /// остаётся в истории (чтобы не сдвигались индексы и репорты на него
    /// оставались валидными), но Text стирается на сервере и iOS рендерит
    /// «[Сообщение удалено]».
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Apartment? Apartment { get; set; }
    public User? Sender { get; set; }
}