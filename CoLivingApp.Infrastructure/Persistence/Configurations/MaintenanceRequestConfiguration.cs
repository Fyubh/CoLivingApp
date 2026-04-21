using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.ToTable("MaintenanceRequests");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.PhotoUrl).HasMaxLength(500);
        builder.Property(m => m.CompletionPhotoUrl).HasMaxLength(500);
        builder.Property(m => m.CompletionNotes).HasMaxLength(1000);
        builder.Property(m => m.ResidentFeedback).HasMaxLength(1000);

        builder.Property(m => m.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

        // ===== RELATIONS =====

        // Обязательная связь с Building. Restrict — нельзя случайно удалить здание с висящими заявками.
        builder.HasOne(m => m.Building)
            .WithMany()
            .HasForeignKey(m => m.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Опциональная связь с Apartment.
        builder.HasOne(m => m.Apartment)
            .WithMany()
            .HasForeignKey(m => m.ApartmentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Опциональная связь с Room.
        builder.HasOne(m => m.Room)
            .WithMany()
            .HasForeignKey(m => m.RoomId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Жилец-автор заявки. Restrict, чтобы удаление юзера не обрушило историю.
        builder.HasOne(m => m.ReportedByUser)
            .WithMany()
            .HasForeignKey(m => m.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Назначение подрядчика. SetNull — если StaffAssignment деактивируется/удаляется,
        // заявка остаётся в системе с разорванной связью (админ переназначит).
        builder.HasOne(m => m.AssignedStaffAssignment)
            .WithMany()
            .HasForeignKey(m => m.AssignedStaffAssignmentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ===== INDEXES =====

        // "Все заявки здания" + фильтр по статусу — самый частый запрос в админке.
        builder.HasIndex(m => new { m.BuildingId, m.Status });

        // "Мои заявки" — запрос жильца.
        builder.HasIndex(m => m.ReportedByUserId);

        // "Задачи подрядчика" — запрос подрядчика.
        builder.HasIndex(m => new { m.AssignedStaffAssignmentId, m.Status });

        // Сортировка по дате создания (стандартный ORDER BY CreatedAt DESC).
        builder.HasIndex(m => m.CreatedAt);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}