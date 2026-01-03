using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class UseOnlyTradeIdForStockUniqueness : Migration
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_MFUnique",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_StockUnique",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_MFUnique",
                table: "Transactions",
                columns: new[] { "PortfolioId", "ISIN", "TradeDate", "TradeType", "Quantity", "Price" },
                unique: true,
                filter: "[TradeID] IS NULL AND [OrderID] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_StockUnique",
                table: "Transactions",
                columns: new[] { "PortfolioId", "TradeID", "OrderID" },
                unique: true,
                filter: "[TradeID] IS NOT NULL AND [OrderID] IS NOT NULL");
        }
    }
}
