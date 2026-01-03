using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTransactionHashMakeIdsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionHash",
                table: "Transactions");

            // First, make columns nullable
            migrationBuilder.AlterColumn<long>(
                name: "TradeID",
                table: "Transactions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "OrderID",
                table: "Transactions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            // Then convert existing 0 values to NULL
            migrationBuilder.Sql(@"
                UPDATE Transactions 
                SET TradeID = NULL 
                WHERE TradeID = 0
            ");

            migrationBuilder.Sql(@"
                UPDATE Transactions 
                SET OrderID = NULL 
                WHERE OrderID = 0
            ");

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
                filter: "[TradeID] IS NULL AND [OrderID] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_StockUnique",
                table: "Transactions",
                columns: new[] { "PortfolioId", "TradeID", "OrderID" },
                unique: true,
                filter: "[TradeID] IS NOT NULL AND [OrderID] IS NOT NULL");
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

            // Convert NULL back to 0
            migrationBuilder.Sql(@"
                UPDATE Transactions 
                SET TradeID = 0 
                WHERE TradeID IS NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE Transactions 
                SET OrderID = 0 
                WHERE OrderID IS NULL
            ");

            migrationBuilder.AlterColumn<long>(
                name: "TradeID",
                table: "Transactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OrderID",
                table: "Transactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ISIN",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "TransactionHash",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
