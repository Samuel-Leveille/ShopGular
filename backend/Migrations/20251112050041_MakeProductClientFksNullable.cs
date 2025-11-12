using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductClientFksNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_BoughtByClientFK",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_InShoppingCartByClientFK",
                table: "Products");

            migrationBuilder.AlterColumn<long>(
                name: "InShoppingCartByClientFK",
                table: "Products",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "BoughtByClientFK",
                table: "Products",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_BoughtByClientFK",
                table: "Products",
                column: "BoughtByClientFK",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_InShoppingCartByClientFK",
                table: "Products",
                column: "InShoppingCartByClientFK",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.AlterColumn<long>(
                name: "InShoppingCartByClientFK",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "BoughtByClientFK",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

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
        }
    }
}
