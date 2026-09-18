using FinanSmart.Api.Entities; using Microsoft.EntityFrameworkCore; using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace FinanSmart.Api.Data.Configurations;
public class InvestmentProductConfiguration : IEntityTypeConfiguration<InvestmentProduct>
{
 public void Configure(EntityTypeBuilder<InvestmentProduct> builder) { builder.Property(x=>x.MinimumAmount).HasPrecision(18,2); builder.Property(x=>x.MaximumAmount).HasPrecision(18,2); builder.HasIndex(x=>x.Name); }
}
