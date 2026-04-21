using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Number).IsRequired().HasMaxLength(10);

        // Enum'ы храним как читаемые строки, чтобы БД была понятна при ручном просмотре
        // (этого принципа уже придерживаются другие entities в проекте).
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(30);

        builder.Property(r => r.SquareMeters).HasColumnType("decimal(6,2)");
        builder.Property(r => r.MonthlyRent).HasColumnType("decimal(10,2)");

        // Отношение: Apartment 1 -> N Rooms.
        builder.HasOne(r => r.Apartment)
            .WithMany(a => a.Rooms)
            .HasForeignKey(r => r.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // В одной квартире не может быть двух комнат с одинаковым номером.
        builder.HasIndex(r => new { r.ApartmentId, r.Number }).IsUnique();

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}