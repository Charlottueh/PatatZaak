using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatatZaak.Migrations
{
    /// <inheritdoc />
    public partial class AddToDatabaseFour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PickupNumber",
                table: "Order",
                type: "int",
                maxLength: 10,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupTime",
                table: "Order",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickupNumber",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PickupTime",
                table: "Order");
        }
    }
}
