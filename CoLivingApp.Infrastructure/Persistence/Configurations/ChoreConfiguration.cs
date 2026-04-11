using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class ChoreConfiguration : IEntityTypeConfiguration<Chore>
{
    public void Configure(EntityTypeBuilder<Chore> builder)
    {
        builder.HasKey(c => c.Id);

        // Конвертируем Enum статуса в удобный текст для БД
        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        // Принудительно ограничиваем длину названия задачи
        builder.Property(c => c.Title).HasMaxLength(200);
    }
}