using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanSmart.Api.Data.Configurations;

public class InvestmentApplicationConfiguration : IEntityTypeConfiguration<InvestmentApplication>
{
    public void Configure(EntityTypeBuilder<InvestmentApplication> builder)
    {
        builder.HasIndex(application => application.ApplicationNumber).IsUnique();
        builder.Property(application => application.Amount).HasPrecision(18, 2);
        builder.Property(application => application.AnnualInterestRate).HasPrecision(9, 4);
        builder.Property(application => application.TotalInterest).HasPrecision(18, 2);
        builder.Property(application => application.PrincipalAtMaturity).HasPrecision(18, 2);
        builder.Property(application => application.TotalReceived).HasPrecision(18, 2);
        builder.HasOne(application => application.InvestmentProduct)
            .WithMany(product => product.InvestmentApplications)
            .HasForeignKey(application => application.InvestmentProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(application => application.User)
            .WithMany(user => user.InvestmentApplications)
            .HasForeignKey(application => application.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
