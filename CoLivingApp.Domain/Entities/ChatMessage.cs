using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// К какому контексту привязано сообщение. Соответствующий *Id должен быть не-null,
    /// два других — null. Инвариант защищён CHECK-constraint.
    /// </summary>
    public ChatScope Scope { get; set; } = ChatScope.Apartment;

    public Guid? ApartmentId { get; set; }
    public Guid? BuildingId { get; set; }

    /// <summary>
    /// FK на Event появится в фазе 8.3 одновременно с самой сущностью Event.
    /// До тех пор хранится как обычная колонка без внешнего ключа.
    /// </summary>
    public Guid? EventId { get; set; }

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
    public Building? Building { get; set; }
    public User? Sender { get; set; }
}
