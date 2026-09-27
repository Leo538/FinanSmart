using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanSmart.Api.Data.Configurations;

public class CreditRateConfiguration : IEntityTypeConfiguration<CreditRate>
{
    public void Configure(EntityTypeBuilder<CreditRate> builder)
    {
        builder.Property(creditRate => creditRate.AnnualInterestRate).HasPrecision(9, 6);

        builder.HasOne(creditRate => creditRate.CreditType)
            .WithMany(creditType => creditType.CreditRates)
            .HasForeignKey(creditRate => creditRate.CreditTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

