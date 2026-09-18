using FinanSmart.Api.Entities; using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace FinanSmart.Api.Data.Configurations;
public class InvestmentRateConfiguration : IEntityTypeConfiguration<InvestmentRate>
{
 public void Configure(EntityTypeBuilder<InvestmentRate> builder) { builder.Property(x=>x.MinimumAmount).HasPrecision(18,2); builder.Property(x=>x.MaximumAmount).HasPrecision(18,2); builder.Property(x=>x.AnnualInterestRate).HasPrecision(9,6); builder.HasIndex(x=>x.InvestmentProductId); builder.HasOne(x=>x.InvestmentProduct).WithMany(x=>x.InvestmentRates).HasForeignKey(x=>x.InvestmentProductId).OnDelete(DeleteBehavior.Restrict); }
}
