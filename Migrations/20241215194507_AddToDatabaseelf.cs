using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatatZaak.Migrations
{
    /// <inheritdoc />
    public partial class AddToDatabaseelf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addon_Product_ProductId",
                table: "Addon");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addon",
                table: "Addon");

            migrationBuilder.RenameTable(
                name: "Addon",
                newName: "Addons");

            migrationBuilder.RenameIndex(
                name: "IX_Addon_ProductId",
                table: "Addons",
                newName: "IX_Addons_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addons",
                table: "Addons",
                column: "Identifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Addons_Product_ProductId",
                table: "Addons",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addons_Product_ProductId",
                table: "Addons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addons",
                table: "Addons");

            migrationBuilder.RenameTable(
                name: "Addons",
                newName: "Addon");

            migrationBuilder.RenameIndex(
                name: "IX_Addons_ProductId",
                table: "Addon",
                newName: "IX_Addon_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addon",
                table: "Addon",
                column: "Identifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Addon_Product_ProductId",
                table: "Addon",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
