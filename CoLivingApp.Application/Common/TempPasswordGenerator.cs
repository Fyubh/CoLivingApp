using System.Security.Cryptography;

namespace CoLivingApp.Application.Common;

/// <summary>
/// Генератор временных паролей для онбординг-flow.
///
/// Алфавит — 32 читаемых символа (без 0/O, l/1, без заглавных, чтобы при
/// диктовке голосом и при наборе вручную не было путаницы).
///
/// Длина 12: при алфавите 32 даёт 12 * log2(32) = 60 бит энтропии.
/// Этого достаточно для одноразового пароля, который живёт максимум неделю
/// до первой смены пароля жильцом.
///
/// Использует RandomNumberGenerator (CSPRNG), а не System.Random, потому что
/// пароль попадает в БД и видится глазами админа — должен быть непредсказуем.
/// </summary>
public static class TempPasswordGenerator
{
    private const string Alphabet = "abcdefghjkmnpqrstuvwxyz23456789";
    // Проверка: 31 символ. Перепроверяем константу:
    //  a b c d e f g h j k m n p q r s t u v w x y z 2 3 4 5 6 7 8 9
    //  1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31
    // ОК, 31 символ. Энтропия чуть меньше идеала (12 * log2(31) ≈ 59.5 бит) —
    // всё равно с большим запасом для одноразового пароля.

    public const int DefaultLength = 12;

    /// <summary>
    /// Генерирует случайный пароль.
    /// </summary>
    /// <param name="length">Длина пароля. По умолчанию 12.</param>
    public static string Generate(int length = DefaultLength)
    {
        if (length < 8)
            throw new ArgumentOutOfRangeException(nameof(length),
                "Минимальная длина временного пароля — 8 символов.");

        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            // GetInt32(maxExclusive) — равномерное распределение, без modulo bias.
            var idx = RandomNumberGenerator.GetInt32(Alphabet.Length);
            chars[i] = Alphabet[idx];
        }
        return new string(chars);
    }
}