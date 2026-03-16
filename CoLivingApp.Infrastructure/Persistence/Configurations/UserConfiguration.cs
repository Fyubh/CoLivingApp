using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Указываем, что свойство Id является первичным ключом (Primary Key)
        builder.HasKey(u => u.Id);
        
        // Настраиваем колонку Email: обязательна для заполнения (NOT NULL) 
        // и имеет максимальную длину 256 символов (как в стандарте ASP.NET Identity)
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        
        // Настраиваем колонку Name
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
    }
}