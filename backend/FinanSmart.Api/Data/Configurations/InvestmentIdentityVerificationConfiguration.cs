using FinanSmart.Api.Entities; using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace FinanSmart.Api.Data.Configurations;
public class InvestmentIdentityVerificationConfiguration : IEntityTypeConfiguration<InvestmentIdentityVerification>
{
 public void Configure(EntityTypeBuilder<InvestmentIdentityVerification> builder)
 {
  builder.HasIndex(x=>x.InvestmentApplicationId).IsUnique();
  builder.HasOne(x=>x.InvestmentApplication).WithOne(x=>x.IdentityVerification).HasForeignKey<InvestmentIdentityVerification>(x=>x.InvestmentApplicationId).OnDelete(DeleteBehavior.Restrict);
 }
}
