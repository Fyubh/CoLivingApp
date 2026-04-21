using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.ToTable("Floors");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name).HasMaxLength(100);

        // Отношение: Building 1 -> N Floors.
        // Cascade — при soft-delete здания этажи идут следом (но сами soft-deleted не каскадятся,
        // физический delete здания снесёт этажи).
        builder.HasOne(f => f.Building)
            .WithMany(b => b.Floors)
            .HasForeignKey(f => f.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        // В одном здании не может быть двух этажей с одинаковым номером.
        builder.HasIndex(f => new { f.BuildingId, f.Number }).IsUnique();

        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}