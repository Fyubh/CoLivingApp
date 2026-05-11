using CoLivingApp.Domain.Entities;
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

        // Чтение истории сортируется по SentAt + фильтрует по ApartmentId.
        builder.HasIndex(m => new { m.ApartmentId, m.SentAt });

        builder.HasOne(m => m.Apartment)
            .WithMany()
            .HasForeignKey(m => m.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
