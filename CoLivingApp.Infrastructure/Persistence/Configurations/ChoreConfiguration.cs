using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class ChoreConfiguration : IEntityTypeConfiguration<Chore>
{
    public void Configure(EntityTypeBuilder<Chore> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.Title).HasMaxLength(200);

        // НОВЫЕ ПРАВИЛА 👇
        builder.Property(c => c.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.Description).HasMaxLength(500); // Ограничим длину описания
    }
}