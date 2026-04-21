using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class Incident
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Кто нарушил
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    // Данные от ИИ (заглушки для MVP)
    public string ImageUrl { get; set; } = string.Empty; // Ссылка на фото "до/после"
    public string Description { get; set; } = string.Empty; // Например: "Грязная посуда на кухне 3-го этажа"
    public decimal AiConfidenceScore { get; set; } // Уверенность нейросети (например, 95.5%)

    public IncidentStatus Status { get; set; } = IncidentStatus.PendingAI;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}