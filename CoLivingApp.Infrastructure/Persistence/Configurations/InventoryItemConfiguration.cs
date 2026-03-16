using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(i => i.Id);
        
        // Конвертация Enum в строку (String).
        // По умолчанию EF Core сохраняет Enum как числа (0, 1, 2...). 
        // Сохраняя как текст ("Available", "RunningLow"), мы делаем базу читаемой для людей.
        builder.Property(i => i.Unit)
            .HasConversion<string>()
            .HasMaxLength(50);
               
        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Количество товара. Например 1.500 (килограмма). 
        // Даем 3 знака после запятой для точности в граммах.
        builder.Property(i => i.Quantity).HasColumnType("decimal(10,3)"); 

        // Кто создал запись о товаре (защита от случайного удаления юзера)
        builder.HasOne(i => i.CreatedBy)
            .WithMany()
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Кто последний изменил запись о товаре
        builder.HasOne(i => i.UpdatedBy)
            .WithMany()
            .HasForeignKey(i => i.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}