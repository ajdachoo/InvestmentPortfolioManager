using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentPortfolioManager.Migrations
{
    public partial class Init4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "ClosedValue",
                table: "Positions");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Positions",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Positions",
                newName: "TransactionDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Positions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "Positions",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "Positions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "ClosedValue",
                table: "Positions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
