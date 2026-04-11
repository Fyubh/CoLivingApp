namespace CoLivingApp.Domain.Enums;

public enum ChoreStatus
{
    Pending,       // Задача ждет выполнения
    NeedsReview,   // Выполнена, ждет подтверждения соседями
    Completed      // Подтверждена и закрыта
}