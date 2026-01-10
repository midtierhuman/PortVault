using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFileUploadLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileUploads_PortfolioId_FileHash",
                table: "FileUploads");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_PortfolioId_FileHash",
                table: "FileUploads",
                columns: new[] { "PortfolioId", "FileHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileUploads_PortfolioId_FileHash",
                table: "FileUploads");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_PortfolioId_FileHash",
                table: "FileUploads",
                columns: new[] { "PortfolioId", "FileHash" },
                unique: true);
        }
    }
}
