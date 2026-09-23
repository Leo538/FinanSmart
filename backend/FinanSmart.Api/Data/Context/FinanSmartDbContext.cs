using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Data.Context;

public class FinanSmartDbContext(DbContextOptions<FinanSmartDbContext> options) : DbContext(options)
{
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<CreditType> CreditTypes => Set<CreditType>();
    public DbSet<CreditRate> CreditRates => Set<CreditRate>();
    public DbSet<CreditCharge> CreditCharges => Set<CreditCharge>();
    public DbSet<CreditInsurance> CreditInsurances => Set<CreditInsurance>();
    public DbSet<CreditTypeInsurance> CreditTypeInsurances => Set<CreditTypeInsurance>();
    public DbSet<InvestmentProduct> InvestmentProducts => Set<InvestmentProduct>();
    public DbSet<InvestmentRate> InvestmentRates => Set<InvestmentRate>();
    public DbSet<InvestmentApplication> InvestmentApplications => Set<InvestmentApplication>();
    public DbSet<InvestmentApplicationDocument> InvestmentApplicationDocuments => Set<InvestmentApplicationDocument>();
    public DbSet<InvestmentIdentityVerification> InvestmentIdentityVerifications => Set<InvestmentIdentityVerification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanSmartDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
