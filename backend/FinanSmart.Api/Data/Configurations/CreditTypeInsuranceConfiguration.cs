using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanSmart.Api.Data.Configurations;

public class CreditTypeInsuranceConfiguration : IEntityTypeConfiguration<CreditTypeInsurance>
{
    public void Configure(EntityTypeBuilder<CreditTypeInsurance> builder)
    {
        builder.HasKey(item => new { item.CreditTypeId, item.CreditInsuranceId });
        builder.HasOne(item => item.CreditType).WithMany(item => item.CreditTypeInsurances)
            .HasForeignKey(item => item.CreditTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.CreditInsurance).WithMany(item => item.CreditTypeInsurances)
            .HasForeignKey(item => item.CreditInsuranceId).OnDelete(DeleteBehavior.Restrict);
    }
}
