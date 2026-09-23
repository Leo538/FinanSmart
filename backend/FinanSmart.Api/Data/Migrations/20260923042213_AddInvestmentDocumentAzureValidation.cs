using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentDocumentAzureValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtractedCountryRegion",
                table: "InvestmentApplicationDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExtractedDateOfBirth",
                table: "InvestmentApplicationDocuments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedDocumentNumber",
                table: "InvestmentApplicationDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedDocumentType",
                table: "InvestmentApplicationDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExtractedExpirationDate",
                table: "InvestmentApplicationDocuments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedFirstName",
                table: "InvestmentApplicationDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedLastName",
                table: "InvestmentApplicationDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ValidatedAt",
                table: "InvestmentApplicationDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationMessage",
                table: "InvestmentApplicationDocuments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationStatus",
                table: "InvestmentApplicationDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CreditInsurances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    InsuranceType = table.Column<string>(type: "text", nullable: false),
                    CalculationBasis = table.Column<string>(type: "text", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    FixedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Frequency = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditInsurances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditTypeInsurances",
                columns: table => new
                {
                    CreditTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditInsuranceId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditTypeInsurances", x => new { x.CreditTypeId, x.CreditInsuranceId });
                    table.ForeignKey(
                        name: "FK_CreditTypeInsurances_CreditInsurances_CreditInsuranceId",
                        column: x => x.CreditInsuranceId,
                        principalTable: "CreditInsurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditTypeInsurances_CreditTypes_CreditTypeId",
                        column: x => x.CreditTypeId,
                        principalTable: "CreditTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditInsurances_Name",
                table: "CreditInsurances",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditTypeInsurances_CreditInsuranceId",
                table: "CreditTypeInsurances",
                column: "CreditInsuranceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditTypeInsurances");

            migrationBuilder.DropTable(
                name: "CreditInsurances");

            migrationBuilder.DropColumn(
                name: "ExtractedCountryRegion",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedDateOfBirth",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedDocumentNumber",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedDocumentType",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedExpirationDate",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedFirstName",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ExtractedLastName",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ValidationMessage",
                table: "InvestmentApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "ValidationStatus",
                table: "InvestmentApplicationDocuments");
        }
    }
}
