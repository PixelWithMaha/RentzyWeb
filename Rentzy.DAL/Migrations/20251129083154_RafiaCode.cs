using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentzy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RafiaCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyApprovalRequests_ApprovalStatus_StatusId",
                table: "PropertyApprovalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyApprovalRequests_Users_AdminId",
                table: "PropertyApprovalRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "PropertyApprovalRequests",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyApprovalRequests_ApprovalStatus_StatusId",
                table: "PropertyApprovalRequests",
                column: "StatusId",
                principalTable: "ApprovalStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyApprovalRequests_Users_AdminId",
                table: "PropertyApprovalRequests",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyApprovalRequests_ApprovalStatus_StatusId",
                table: "PropertyApprovalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyApprovalRequests_Users_AdminId",
                table: "PropertyApprovalRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "PropertyApprovalRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyApprovalRequests_ApprovalStatus_StatusId",
                table: "PropertyApprovalRequests",
                column: "StatusId",
                principalTable: "ApprovalStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyApprovalRequests_Users_AdminId",
                table: "PropertyApprovalRequests",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
