using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class ResidentNotificationConfiguration : IEntityTypeConfiguration<ResidentNotification>
{
    public void Configure(EntityTypeBuilder<ResidentNotification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Audience).HasConversion<string>().HasMaxLength(30);
        builder.Property(n => n.Title).HasMaxLength(160).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(2000).IsRequired();
        builder.Property(n => n.UserId).HasMaxLength(64).IsRequired();

        builder.HasOne(n => n.Building)
            .WithMany()
            .HasForeignKey(n => n.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UserId, n.CreatedAt });
        builder.HasIndex(n => new { n.BuildingId, n.CreatedAt });
        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}
