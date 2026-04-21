namespace CoLivingApp.Domain.Enums;

public enum IncidentStatus
{
    PendingAI = 1,    // Ожидает проверки админом (ИИ нашел нарушение)
    Approved = 2,     // Админ подтвердил (Штраф выписан)
    Rejected = 3,     // Админ отклонил (ИИ ошибся)
    Appealed = 4      // Студент оспаривает
}