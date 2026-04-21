using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);

        // Настройки новых полей
        builder.Property(u => u.Role)
            .HasConversion<string>() // Сохраняем Enum как строку ("Tenant", "Admin")
            .IsRequired();

        builder.Property(u => u.AccessLevel)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(u => u.KarmaScore)
            .HasDefaultValue(100) // Стартовая карма для всех — 100 баллов
            .IsRequired();
    }
}