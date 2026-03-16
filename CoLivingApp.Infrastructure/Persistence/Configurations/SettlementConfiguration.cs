using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Amount).HasColumnType("decimal(18,2)");

        // ВАЖНАЯ НАСТРОЙКА: Таблица Возвратов долгов ссылается на таблицу Users ДВАЖДЫ 
        // (как Отправитель и как Получатель). 
        // Если здесь не поставить DeleteBehavior.Restrict, PostgreSQL выдаст ошибку 
        // при создании таблиц из-за "множественных каскадных путей" (цикл удаления).

        // Настройка связи для Отправителя денег
        builder.HasOne(s => s.Sender)
            .WithMany()
            .HasForeignKey(s => s.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Настройка связи для Получателя денег
        builder.HasOne(s => s.Receiver)
            .WithMany()
            .HasForeignKey(s => s.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}