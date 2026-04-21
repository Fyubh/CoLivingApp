using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.AddressLine).IsRequired().HasMaxLength(300);
        builder.Property(b => b.City).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Country).IsRequired().HasMaxLength(2); // ISO-код
        builder.Property(b => b.PostalCode).HasMaxLength(20);
        builder.Property(b => b.TimeZone).HasMaxLength(50);

        builder.Property(b => b.Latitude).HasColumnType("decimal(10,7)");
        builder.Property(b => b.Longitude).HasColumnType("decimal(10,7)");

        // Отношение: Operator 1 -> N Buildings.
        // Restrict — нельзя удалить оператора, если у него есть здания.
        builder.HasOne(b => b.Operator)
            .WithMany(o => o.Buildings)
            .HasForeignKey(b => b.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.OperatorId);
        // Индекс под запрос "все здания оператора X в городе Y".
        builder.HasIndex(b => new { b.Country, b.City });

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}