using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatatZaak.Migrations
{
    /// <inheritdoc />
    public partial class AddToDatabasefive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProductPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                 defaultValue: 0);


            migrationBuilder.AddColumn<int>(
                name: "ProductQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                 defaultValue: 0);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductPrice",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProductQuantity",
                table: "Product");
        }
    }
}
