using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration для Apartment. Раньше настройки шли через конвенции EF Core,
/// но с добавлением FloorId/BuildingId нужно явно описать optional relationships
/// и индексы, чтобы продакшн-запросы работали быстро.
/// </summary>
public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        builder.ToTable("Apartments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.InviteCode).HasMaxLength(10);
        builder.Property(a => a.UnitNumber).HasMaxLength(20);

        // Индекс по InviteCode — используется в JoinApartmentCommandHandler
        // (до сих пор он работал через конвенцию без индекса — добавляем явно).
        builder.HasIndex(a => a.InviteCode);

        // Optional FK: Apartment может жить БЕЗ этажа (consumer mode) или быть привязан к этажу (B2B mode).
        // Restrict — нельзя удалить Floor, если в нём есть квартиры.
        builder.HasOne(a => a.Floor)
            .WithMany(f => f.Apartments)
            .HasForeignKey(a => a.FloorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Денормализованная ссылка на Building — для быстрых запросов "все квартиры в здании".
        builder.HasOne(a => a.Building)
            .WithMany()
            .HasForeignKey(a => a.BuildingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Критический индекс для SaaS: "покажи мне все квартиры моего здания".
        // Без него запрос с 50k квартир в системе на одно здание будет делать full scan.
        builder.HasIndex(a => a.BuildingId);
    }
}