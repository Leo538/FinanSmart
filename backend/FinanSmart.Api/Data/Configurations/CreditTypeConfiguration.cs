using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanSmart.Api.Data.Configurations;

public class CreditTypeConfiguration : IEntityTypeConfiguration<CreditType>
{
    public void Configure(EntityTypeBuilder<CreditType> builder)
    {
        builder.Property(creditType => creditType.MinimumAmount).HasPrecision(18, 2);
        builder.Property(creditType => creditType.MaximumAmount).HasPrecision(18, 2);
    }
}

