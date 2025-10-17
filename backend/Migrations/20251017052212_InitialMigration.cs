using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BoughtByClientFK",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "InShoppingCartByClientFK",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SellerId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_BoughtByClientFK",
                table: "Products",
                column: "BoughtByClientFK");

            migrationBuilder.CreateIndex(
                name: "IX_Products_InShoppingCartByClientFK",
                table: "Products",
                column: "InShoppingCartByClientFK");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerId",
                table: "Products",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_BoughtByClientFK",
                table: "Products",
                column: "BoughtByClientFK",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_InShoppingCartByClientFK",
                table: "Products",
                column: "InShoppingCartByClientFK",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_BoughtByClientFK",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_InShoppingCartByClientFK",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_BoughtByClientFK",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_InShoppingCartByClientFK",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BoughtByClientFK",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "InShoppingCartByClientFK",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Products");
        }
    }
}
