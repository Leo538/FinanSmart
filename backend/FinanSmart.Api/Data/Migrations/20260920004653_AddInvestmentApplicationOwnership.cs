using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentApplicationOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "InvestmentApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentApplications_UserId",
                table: "InvestmentApplications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentApplications_Users_UserId",
                table: "InvestmentApplications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentApplications_Users_UserId",
                table: "InvestmentApplications");

            migrationBuilder.DropIndex(
                name: "IX_InvestmentApplications_UserId",
                table: "InvestmentApplications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InvestmentApplications");
        }
    }
}
