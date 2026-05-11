using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class ChatBlockConfiguration : IEntityTypeConfiguration<ChatBlock>
{
    public void Configure(EntityTypeBuilder<ChatBlock> builder)
    {
        builder.HasKey(b => b.Id);

        // Одна и та же пара (Blocker → Blocked) хранится один раз.
        // Block / Unblock команда полагается на этот индекс для идемпотентности.
        builder.HasIndex(b => new { b.BlockerId, b.BlockedUserId }).IsUnique();

        builder.HasOne(b => b.Blocker)
            .WithMany()
            .HasForeignKey(b => b.BlockerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.BlockedUser)
            .WithMany()
            .HasForeignKey(b => b.BlockedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
