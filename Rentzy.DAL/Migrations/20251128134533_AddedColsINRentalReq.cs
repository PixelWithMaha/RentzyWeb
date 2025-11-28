using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rentzy.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedColsINRentalReq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "PropertyRentalRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "PropertyRentalRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "PropertyRentalRequests");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PropertyRentalRequests");
        }
    }
}
