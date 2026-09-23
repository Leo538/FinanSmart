using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanSmart.Api.Data.Configurations;

public class CreditInsuranceConfiguration : IEntityTypeConfiguration<CreditInsurance>
{
    public void Configure(EntityTypeBuilder<CreditInsurance> builder)
    {
        builder.Property(item => item.Name).HasMaxLength(160).IsRequired();
        builder.Property(item => item.InsuranceType).HasConversion<string>();
        builder.Property(item => item.CalculationBasis).HasConversion<string>();
        builder.Property(item => item.Frequency).HasConversion<string>();
        builder.Property(item => item.Rate).HasPrecision(18, 6);
        builder.Property(item => item.FixedAmount).HasPrecision(18, 2);
        builder.HasIndex(item => item.Name).IsUnique();
    }
}
