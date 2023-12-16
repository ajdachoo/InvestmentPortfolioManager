using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentPortfolioManager.Migrations
{
    public partial class Init3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Positions",
                newName: "ClosedDate");

            migrationBuilder.AddColumn<decimal>(
                name: "ClosedValue",
                table: "Positions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedValue",
                table: "Positions");

            migrationBuilder.RenameColumn(
                name: "ClosedDate",
                table: "Positions",
                newName: "UpdatedDate");
        }
    }
}
