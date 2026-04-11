using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);

        // ВАЖНО: Финансы (деньги) всегда храним в формате decimal(18, 2) в БД.
        // Это значит 18 цифр всего, из них 2 после запятой. 
        // Если оставить default, база может округлить копейки, и баланс соседей не сойдется!
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        
        // Ограничиваем длину описания чека, чтобы в БД не записали "Войну и мир"
        builder.Property(e => e.Description).HasMaxLength(500);

        // Связь 1-ко-многим: Один Чек (Expense) содержит много Долей (Splits)
        builder.HasMany(e => e.Splits)
            .WithOne(s => s.Expense)
            .HasForeignKey(s => s.ExpenseId)
            // Cascade означает: если мы удаляем Чек из БД, 
            // все его "доли" удалятся автоматически (чтобы не было "висячих" записей).
            .OnDelete(DeleteBehavior.Cascade); 

        // Связь 1-ко-многим: У чека есть один Плательщик (Payer)
        builder.HasOne(e => e.Payer)
            .WithMany(u => u.PaidExpenses)
            .HasForeignKey(e => e.PayerId)
            // Restrict означает: БД не даст удалить Юзера, если за ним числятся чеки.
            // Это защищает нас от потери финансовой истории.
            .OnDelete(DeleteBehavior.Restrict); 
        // Сохраняем категорию как текст (Rent, Utilities и т.д.)
        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}