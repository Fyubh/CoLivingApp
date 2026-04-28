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

        // Уникальный индекс по email — иначе можно создать двух пользователей
        // с одинаковым email и логин сломается. Filter добавляем чтобы не падали
        // тесты, если в системе уже есть дубли (мало вероятно, но безопасно).
        builder.HasIndex(u => u.Email).IsUnique();

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

        // Onboarding: при создании через сидер/register флаг false (обычный юзер).
        // Через CreateUser-handler админ выставляет true, чтобы заставить сменить
        // временный пароль при первом входе.
        builder.Property(u => u.MustChangePassword)
            .HasDefaultValue(false)
            .IsRequired();
    }
}