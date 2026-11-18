using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiAvto.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientCardsAndTariffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CarListingId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CountryName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    DestinationCity = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    DeliveryToVladivostokTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DeliveryFromVladivostokTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    InspectionTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CarPriceCny = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    MarkupPercent = table.Column<decimal>(type: "TEXT", precision: 9, scale: 2, nullable: false),
                    CarPriceRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DeliveryToVladivostokRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DeliveryFromVladivostokRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    InspectionRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientCards_CarListings_CarListingId",
                        column: x => x.CarListingId,
                        principalTable: "CarListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTariffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AmountRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTariffs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientCards_CarListingId_CreatedAt",
                table: "ClientCards",
                columns: new[] { "CarListingId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientCards_CreatedAt",
                table: "ClientCards",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCards_PublicId",
                table: "ClientCards",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTariffs_Kind_IsActive_SortOrder",
                table: "ServiceTariffs",
                columns: new[] { "Kind", "IsActive", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientCards");

            migrationBuilder.DropTable(
                name: "ServiceTariffs");
        }
    }
}
