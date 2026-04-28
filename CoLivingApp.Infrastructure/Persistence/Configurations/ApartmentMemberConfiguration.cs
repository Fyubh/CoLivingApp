using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration для ApartmentMember. Раньше работала через конвенции EF Core,
/// но с добавлением RoomId нужно явно настроить optional FK на Room и индекс
/// для быстрого поиска "кто живёт в этой комнате прямо сейчас".
/// </summary>
public class ApartmentMemberConfiguration : IEntityTypeConfiguration<ApartmentMember>
{
    public void Configure(EntityTypeBuilder<ApartmentMember> builder)
    {
        builder.ToTable("ApartmentMembers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId).IsRequired();

        // FK на User (явно описываем, чтобы при появлении нового FK на Room
        // EF не запутался в навигациях).
        builder.HasOne(m => m.User)
            .WithMany(u => u.ApartmentMembers)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK на Apartment.
        builder.HasOne(m => m.Apartment)
            .WithMany(a => a.Members)
            .HasForeignKey(m => m.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // НОВОЕ: optional FK на Room.
        // Restrict — если в комнате есть жильцы (даже бывшие, исторические),
        // удалить комнату нельзя. Бывших жильцов сначала переносят в архив
        // или обнуляют RoomId — это решение продукта, а не БД.
        builder.HasOne(m => m.Room)
            .WithMany() // у Room пока нет коллекции Members — добавим, если станет нужно
            .HasForeignKey(m => m.RoomId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Критический индекс для проверки "комната занята":
        // при создании жильца handler делает запрос
        //   WHERE RoomId == X AND IsActive == true
        // Этот индекс делает такой запрос O(log n) даже на большой базе.
        builder.HasIndex(m => new { m.RoomId, m.IsActive });
    }
}