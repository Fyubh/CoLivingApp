using CoLivingApp.Domain.Common;

namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Оператор — это SaaS-клиент, которому мы продаём продукт.
/// Например: "The Fizz Group" владеет зданиями в Праге, Мюнхене, Вене.
/// 
/// Над оператором — только супер-админ (сам Anthropic/мы).
/// Под оператором — одно или несколько Buildings.
/// 
/// Оператор отвечает за:
/// - Биллинг (мы выставляем ему счёт за всех активных резидентов в его зданиях).
/// - Брендирование (логотип, цвета — white-label).
/// - Глобальных админов, которые имеют доступ ко всем его зданиям.
/// </summary>
public class Operator : EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Полное название ("The Fizz Group GmbH").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Короткий идентификатор для URL и white-label ("the-fizz"). Уникальный.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Контактный email для биллинга и системных уведомлений.</summary>
    public string? ContactEmail { get; set; }

    /// <summary>Ссылка на логотип (для админки и white-label фронта).</summary>
    public string? LogoUrl { get; set; }

    /// <summary>HEX-цвет брендинга (например "#1E6FEA"). Опционально.</summary>
    public string? BrandPrimaryColor { get; set; }

    /// <summary>HEX-цвет акцента. Опционально.</summary>
    public string? BrandSecondaryColor { get; set; }

    /// <summary>Оператор активен — принимает ли платежи и видна ли его админка.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Building> Buildings { get; set; } = new List<Building>();
}