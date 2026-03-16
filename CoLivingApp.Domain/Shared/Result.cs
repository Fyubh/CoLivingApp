// Файл: CoLivingApp.Domain/Shared/Result.cs
namespace CoLivingApp.Domain.Shared;

/// <summary>
/// Класс для элегантной обработки результатов выполнения бизнес-логики без использования Exceptions.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    // Успешный результат
    public static Result<T> Success(T value) => new(true, value, null);
    
    // Результат с ошибкой
    public static Result<T> Failure(string error) => new(false, default, error);
}