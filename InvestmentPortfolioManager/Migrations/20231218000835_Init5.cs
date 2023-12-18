using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentPortfolioManager.Migrations
{
    public partial class Init5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Assets_AssetId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Wallets_WalletId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_OwnerId",
                table: "Wallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "Transactions");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Wallets",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Wallets_OwnerId",
                table: "Wallets",
                newName: "IX_Wallets_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_WalletId",
                table: "Transactions",
                newName: "IX_Transactions_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_AssetId",
                table: "Transactions",
                newName: "IX_Transactions_AssetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Assets_AssetId",
                table: "Transactions",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Users_UserId",
                table: "Wallets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Assets_AssetId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_UserId",
                table: "Wallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Positions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Wallets",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                newName: "IX_Wallets_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_WalletId",
                table: "Positions",
                newName: "IX_Positions_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AssetId",
                table: "Positions",
                newName: "IX_Positions_AssetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Assets_AssetId",
                table: "Positions",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Wallets_WalletId",
                table: "Positions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Users_OwnerId",
                table: "Wallets",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
