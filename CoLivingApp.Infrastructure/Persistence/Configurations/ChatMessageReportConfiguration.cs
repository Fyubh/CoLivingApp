using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class ChatMessageReportConfiguration : IEntityTypeConfiguration<ChatMessageReport>
{
    public void Configure(EntityTypeBuilder<ChatMessageReport> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Reason).HasMaxLength(500);

        // Один жилец может зарепортить одно сообщение только один раз.
        builder.HasIndex(r => new { r.MessageId, r.ReporterId }).IsUnique();

        builder.HasOne(r => r.Message)
            .WithMany()
            .HasForeignKey(r => r.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
