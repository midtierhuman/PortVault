using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTransactionUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_MFUnique",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_StockUnique",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "ISIN",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ISIN",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_MFUnique",
                table: "Transactions",
                columns: new[] { "PortfolioId", "ISIN", "TradeDate", "TradeType", "Quantity", "Price" },
                unique: true,
                filter: "[TradeID] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_StockUnique",
                table: "Transactions",
                columns: new[] { "PortfolioId", "TradeID" },
                unique: true,
                filter: "[TradeID] IS NOT NULL");
        }
    }
}
