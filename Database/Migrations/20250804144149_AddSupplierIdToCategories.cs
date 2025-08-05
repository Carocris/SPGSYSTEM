using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierIdToCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SupplierId",
                table: "Categories",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Suppliers_SupplierId",
                table: "Categories",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Suppliers_SupplierId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_SupplierId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Categories");
        }
    }
}
