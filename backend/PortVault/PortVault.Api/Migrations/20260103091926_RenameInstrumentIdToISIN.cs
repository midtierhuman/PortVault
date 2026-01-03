using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameInstrumentIdToISIN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InstrumentId",
                table: "Holdings",
                newName: "ISIN");

            migrationBuilder.RenameColumn(
                name: "InstrumentId",
                table: "Assets",
                newName: "ISIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ISIN",
                table: "Holdings",
                newName: "InstrumentId");

            migrationBuilder.RenameColumn(
                name: "ISIN",
                table: "Assets",
                newName: "InstrumentId");
        }
    }
}
