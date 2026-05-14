using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Text).HasMaxLength(2000);
        builder.Property(m => m.IsDeleted).HasDefaultValue(false);

        // Scope хранится как int — простая, дешёвая фильтрация по индексу.
        builder.Property(m => m.Scope)
            .HasConversion<int>()
            .HasDefaultValue(ChatScope.Apartment);

        // По каждому скоупу — свой композитный индекс под чтение истории.
        builder.HasIndex(m => new { m.ApartmentId, m.SentAt });
        builder.HasIndex(m => new { m.BuildingId, m.SentAt });
        builder.HasIndex(m => new { m.EventId, m.SentAt });

        builder.HasOne(m => m.Apartment)
            .WithMany()
            .HasForeignKey(m => m.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Здание не удаляется в обычной эксплуатации; на всякий случай —
        // запрещаем удаление здания, если по нему остался чат.
        builder.HasOne(m => m.Building)
            .WithMany()
            .HasForeignKey(m => m.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Инвариант: ровно одна из трёх ссылок не-null и соответствует Scope.
        // Без этой проверки нечаянная вставка building-сообщения с ApartmentId
        // утечёт во все запросы apartment-чата.
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_ChatMessages_ScopeRef",
            "(\"Scope\" = 1 AND \"ApartmentId\" IS NOT NULL AND \"BuildingId\" IS NULL AND \"EventId\" IS NULL)" +
            " OR (\"Scope\" = 2 AND \"ApartmentId\" IS NULL AND \"BuildingId\" IS NOT NULL AND \"EventId\" IS NULL)" +
            " OR (\"Scope\" = 3 AND \"ApartmentId\" IS NULL AND \"BuildingId\" IS NULL AND \"EventId\" IS NOT NULL)"));
    }
}
