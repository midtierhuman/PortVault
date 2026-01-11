using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorporateActions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentInstrumentId = table.Column<long>(type: "bigint", nullable: false),
                    ChildInstrumentId = table.Column<long>(type: "bigint", nullable: true),
                    RatioNumerator = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    RatioDenominator = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CostPercentageAllocated = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateActions_Instruments_ChildInstrumentId",
                        column: x => x.ChildInstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CorporateActions_Instruments_ParentInstrumentId",
                        column: x => x.ParentInstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_ChildInstrumentId",
                table: "CorporateActions",
                column: "ChildInstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_ExDate",
                table: "CorporateActions",
                column: "ExDate");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_ParentInstrumentId",
                table: "CorporateActions",
                column: "ParentInstrumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorporateActions");
        }
    }
}
