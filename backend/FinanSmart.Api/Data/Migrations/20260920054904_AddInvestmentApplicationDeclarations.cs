using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentApplicationDeclarations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DataProcessingAccepted",
                table: "InvestmentApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeclarationsAcceptedAt",
                table: "InvestmentApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InformationAccuracyAccepted",
                table: "InvestmentApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtherSourceOfFunds",
                table: "InvestmentApplications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceOfFunds",
                table: "InvestmentApplications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TermsAccepted",
                table: "InvestmentApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataProcessingAccepted",
                table: "InvestmentApplications");

            migrationBuilder.DropColumn(
                name: "DeclarationsAcceptedAt",
                table: "InvestmentApplications");

            migrationBuilder.DropColumn(
                name: "InformationAccuracyAccepted",
                table: "InvestmentApplications");

            migrationBuilder.DropColumn(
                name: "OtherSourceOfFunds",
                table: "InvestmentApplications");

            migrationBuilder.DropColumn(
                name: "SourceOfFunds",
                table: "InvestmentApplications");

            migrationBuilder.DropColumn(
                name: "TermsAccepted",
                table: "InvestmentApplications");
        }
    }
}
