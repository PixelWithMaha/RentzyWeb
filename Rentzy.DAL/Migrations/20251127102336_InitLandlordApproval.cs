using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentzy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitLandlordApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LandlordApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LandlordId = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DocumentUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalStatusId = table.Column<int>(type: "int", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandlordApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandlordApprovals_ApprovalStatus_ApprovalStatusId",
                        column: x => x.ApprovalStatusId,
                        principalTable: "ApprovalStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LandlordApprovals_Users_LandlordId",
                        column: x => x.LandlordId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LandlordApprovals_ApprovalStatusId",
                table: "LandlordApprovals",
                column: "ApprovalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LandlordApprovals_LandlordId",
                table: "LandlordApprovals",
                column: "LandlordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandlordApprovals");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
