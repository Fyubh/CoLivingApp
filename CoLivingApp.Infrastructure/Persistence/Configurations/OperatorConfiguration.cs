using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoLivingApp.Infrastructure.Persistence.Configurations;

public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> builder)
    {
        builder.ToTable("Operators");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(o => o.Slug).IsUnique();

        builder.Property(o => o.ContactEmail).HasMaxLength(256);
        builder.Property(o => o.LogoUrl).HasMaxLength(500);
        builder.Property(o => o.BrandPrimaryColor).HasMaxLength(20);
        builder.Property(o => o.BrandSecondaryColor).HasMaxLength(20);

        // Soft delete: все запросы по умолчанию фильтруют удалённые.
        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}