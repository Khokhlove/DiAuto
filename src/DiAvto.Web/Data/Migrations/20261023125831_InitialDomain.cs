using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiAvto.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserPasskeys");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaseCurrency = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    QuoteCurrency = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Rate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsFallback = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceSites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    BaseUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DetailUrlTemplate = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AttributionText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastHealthCheckAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastHealthCheckOk = table.Column<bool>(type: "INTEGER", nullable: true),
                    LastHealthCheckMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceSites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceSiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    NameLatin = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    IconUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brands_SourceSites_SourceSiteId",
                        column: x => x.SourceSiteId,
                        principalTable: "SourceSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarListings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceSiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    BrandName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    SeriesName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    SpecName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    FirstRegistration = table.Column<string>(type: "TEXT", nullable: true),
                    PriceCny = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    GuidancePriceCny = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    MileageKm = table.Column<int>(type: "INTEGER", nullable: true),
                    CityName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Displacement = table.Column<string>(type: "TEXT", nullable: true),
                    Gearbox = table.Column<string>(type: "TEXT", nullable: true),
                    FuelType = table.Column<string>(type: "TEXT", nullable: true),
                    EmissionStandard = table.Column<string>(type: "TEXT", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    TransferCount = table.Column<int>(type: "INTEGER", nullable: true),
                    BodyType = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PhotoCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DealerId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    DealerName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    PublicUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsNewEnergy = table.Column<bool>(type: "INTEGER", nullable: false),
                    PublishedAtText = table.Column<string>(type: "TEXT", nullable: true),
                    LastFetchedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RawJson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarListings_SourceSites_SourceSiteId",
                        column: x => x.SourceSiteId,
                        principalTable: "SourceSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SearchRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SourceSiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    BrandName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    SeriesName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    YearFrom = table.Column<int>(type: "INTEGER", nullable: true),
                    YearTo = table.Column<int>(type: "INTEGER", nullable: true),
                    PriceFromCny = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    PriceToCny = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    MileageMaxKm = table.Column<int>(type: "INTEGER", nullable: true),
                    CityName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    ResultCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    MarkupPercent = table.Column<decimal>(type: "TEXT", precision: 9, scale: 2, nullable: false),
                    CriteriaJson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchRequests_SourceSites_SourceSiteId",
                        column: x => x.SourceSiteId,
                        principalTable: "SourceSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BrandId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    Pinyin = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    IconUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PriceRange = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSeries_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarListingId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarPhotos_CarListings_CarListingId",
                        column: x => x.CarListingId,
                        principalTable: "CarListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SearchResultItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SearchRequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    CarListingId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tier = table.Column<int>(type: "INTEGER", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceCny = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PriceRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchResultItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchResultItems_CarListings_CarListingId",
                        column: x => x.CarListingId,
                        principalTable: "CarListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SearchResultItems_SearchRequests_SearchRequestId",
                        column: x => x.SearchRequestId,
                        principalTable: "SearchRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditLogs_CreatedAt",
                table: "AdminAuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_SourceSiteId_ExternalId",
                table: "Brands",
                columns: new[] { "SourceSiteId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarListings_LastFetchedAt",
                table: "CarListings",
                column: "LastFetchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CarListings_PriceCny",
                table: "CarListings",
                column: "PriceCny");

            migrationBuilder.CreateIndex(
                name: "IX_CarListings_SourceSiteId_ExternalId",
                table: "CarListings",
                columns: new[] { "SourceSiteId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarPhotos_CarListingId_SortOrder",
                table: "CarPhotos",
                columns: new[] { "CarListingId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CarSeries_BrandId_ExternalId",
                table: "CarSeries",
                columns: new[] { "BrandId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_Source_BaseCurrency_QuoteCurrency_EffectiveDate",
                table: "CurrencyRates",
                columns: new[] { "Source", "BaseCurrency", "QuoteCurrency", "EffectiveDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_CreatedAt",
                table: "SearchRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_PublicId",
                table: "SearchRequests",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchRequests_SourceSiteId",
                table: "SearchRequests",
                column: "SourceSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchResultItems_CarListingId",
                table: "SearchResultItems",
                column: "CarListingId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchResultItems_SearchRequestId_CarListingId",
                table: "SearchResultItems",
                columns: new[] { "SearchRequestId", "CarListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchResultItems_SearchRequestId_Tier_Rank",
                table: "SearchResultItems",
                columns: new[] { "SearchRequestId", "Tier", "Rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceSites_Key",
                table: "SourceSites",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAuditLogs");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "CarPhotos");

            migrationBuilder.DropTable(
                name: "CarSeries");

            migrationBuilder.DropTable(
                name: "CurrencyRates");

            migrationBuilder.DropTable(
                name: "SearchResultItems");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "CarListings");

            migrationBuilder.DropTable(
                name: "SearchRequests");

            migrationBuilder.DropTable(
                name: "SourceSites");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "AspNetUserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "BLOB", maxLength: 1024, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPasskeys", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_AspNetUserPasskeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPasskeys_UserId",
                table: "AspNetUserPasskeys",
                column: "UserId");
        }
    }
}
