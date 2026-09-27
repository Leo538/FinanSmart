using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentIdentityVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestmentIdentityVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestmentApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelfieOriginalFileName = table.Column<string>(type: "text", nullable: true),
                    SelfieStoredFileName = table.Column<string>(type: "text", nullable: true),
                    SelfieStoragePath = table.Column<string>(type: "text", nullable: true),
                    SelfieContentType = table.Column<string>(type: "text", nullable: true),
                    SelfieFileSize = table.Column<long>(type: "bigint", nullable: true),
                    ConsentAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentIdentityVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentIdentityVerifications_InvestmentApplications_Inve~",
                        column: x => x.InvestmentApplicationId,
                        principalTable: "InvestmentApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentIdentityVerifications_InvestmentApplicationId",
                table: "InvestmentIdentityVerifications",
                column: "InvestmentApplicationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestmentIdentityVerifications");
        }
    }
}
