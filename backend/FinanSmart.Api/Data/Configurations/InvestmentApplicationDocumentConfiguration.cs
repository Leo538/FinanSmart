using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanSmart.Api.Data.Configurations;

public class InvestmentApplicationDocumentConfiguration : IEntityTypeConfiguration<InvestmentApplicationDocument>
{
    public void Configure(EntityTypeBuilder<InvestmentApplicationDocument> builder)
    {
        builder.Property(document => document.ValidationStatus).HasConversion<string>();
        builder.Property(document => document.ValidationMessage).HasMaxLength(500);
        builder.HasIndex(document => document.InvestmentApplicationId);
        builder.HasIndex(document => new { document.InvestmentApplicationId, document.DocumentType, document.IsActive });
        builder.HasOne(document => document.InvestmentApplication)
            .WithMany(application => application.Documents)
            .HasForeignKey(document => document.InvestmentApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
