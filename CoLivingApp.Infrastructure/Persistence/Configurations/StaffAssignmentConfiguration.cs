using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class StaffAssignmentConfiguration : IEntityTypeConfiguration<StaffAssignment>
{
    public void Configure(EntityTypeBuilder<StaffAssignment> builder)
    {
        builder.ToTable("StaffAssignments");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Role).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.Specialization).HasConversion<string>().HasMaxLength(30);

        // Рейтинг 0.00–5.00
        builder.Property(s => s.AverageRating).HasColumnType("decimal(3,2)");

        builder.HasOne(s => s.User)
            .WithMany() // У User не добавляем коллекцию StaffAssignments — оставим чистым
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Building)
            .WithMany() // Аналогично у Building — назначения выгружаются явным запросом
            .HasForeignKey(s => s.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один и тот же юзер не может иметь ДВЕ активных записи одной и той же Role в одном здании.
        // Фильтруем через HasFilter по Postgres-синтаксису.
        builder.HasIndex(s => new { s.UserId, s.BuildingId, s.Role })
            .IsUnique()
            .HasFilter("\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");

        // Индекс под частый запрос "все активные staff в здании X с ролью Y"
        builder.HasIndex(s => new { s.BuildingId, s.Role, s.IsActive });

        // Индекс для быстрого поиска "все назначения пользователя"
        builder.HasIndex(s => s.UserId);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}