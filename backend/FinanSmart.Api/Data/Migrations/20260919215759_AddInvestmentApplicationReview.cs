using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentApplicationReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "InvestmentApplications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "InvestmentApplications",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "InvestmentApplications");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "InvestmentApplications");
        }
    }
}
