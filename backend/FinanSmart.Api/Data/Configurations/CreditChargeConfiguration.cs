using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanSmart.Api.Data.Configurations;

public class CreditChargeConfiguration : IEntityTypeConfiguration<CreditCharge>
{
    public void Configure(EntityTypeBuilder<CreditCharge> builder)
    {
        builder.Property(creditCharge => creditCharge.Value).HasPrecision(18, 6);
        builder.Property(creditCharge => creditCharge.ChargeType).HasConversion<string>();
        builder.Property(creditCharge => creditCharge.Frequency).HasConversion<string>();

        builder.HasOne(creditCharge => creditCharge.CreditType)
            .WithMany(creditType => creditType.CreditCharges)
            .HasForeignKey(creditCharge => creditCharge.CreditTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

