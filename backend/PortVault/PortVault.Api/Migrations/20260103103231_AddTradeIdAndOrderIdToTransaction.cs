using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeIdAndOrderIdToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderID",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TradeID",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderID",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TradeID",
                table: "Transactions");
        }
    }
}
