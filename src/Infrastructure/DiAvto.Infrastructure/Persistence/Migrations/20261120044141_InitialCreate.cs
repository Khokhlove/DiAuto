using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiAvto.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentSectionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Section = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IconName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentSectionItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Page = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Subtitle = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentSections", x => x.Id);
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
                name: "FaqItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Question = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    IsProcessed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ManagerComment = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Offices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    City = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Phone2 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Latitude = table.Column<decimal>(type: "TEXT", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "TEXT", precision: 9, scale: 6, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    AuthorName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    ScreenshotUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    VideoUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    VideoTitle = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "ShowcaseCars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Make = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Fuel = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    BodyType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Transmission = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Drive = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Engine = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    MileageKm = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    InStock = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcaseCars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Key);
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShowcaseCarPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShowcaseCarId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowcaseCarPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowcaseCarPhotos_ShowcaseCars_ShowcaseCarId",
                        column: x => x.ShowcaseCarId,
                        principalTable: "ShowcaseCars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    CustomsClearanceTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CarPriceCny = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    MarkupPercent = table.Column<decimal>(type: "TEXT", precision: 9, scale: 2, nullable: false),
                    CarPriceRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DeliveryToVladivostokRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DeliveryFromVladivostokRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    InspectionRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CustomsClearanceRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalRub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

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
                name: "IX_ContentSectionItems_Section_SortOrder",
                table: "ContentSectionItems",
                columns: new[] { "Section", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentSections_Page_Slug",
                table: "ContentSections",
                columns: new[] { "Page", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_Source_BaseCurrency_QuoteCurrency_EffectiveDate",
                table: "CurrencyRates",
                columns: new[] { "Source", "BaseCurrency", "QuoteCurrency", "EffectiveDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaqItems_IsVisible_SortOrder",
                table: "FaqItems",
                columns: new[] { "IsVisible", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CreatedAt",
                table: "Leads",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_IsProcessed",
                table: "Leads",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_Offices_SortOrder",
                table: "Offices",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_IsVisible_SortOrder",
                table: "Reviews",
                columns: new[] { "IsVisible", "SortOrder" });

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
                name: "IX_ServiceTariffs_Kind_IsActive_SortOrder",
                table: "ServiceTariffs",
                columns: new[] { "Kind", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseCarPhotos_ShowcaseCarId_SortOrder",
                table: "ShowcaseCarPhotos",
                columns: new[] { "ShowcaseCarId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseCars_IsPublished_SortOrder",
                table: "ShowcaseCars",
                columns: new[] { "IsPublished", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseCars_Make",
                table: "ShowcaseCars",
                column: "Make");

            migrationBuilder.CreateIndex(
                name: "IX_ShowcaseCars_PriceRub",
                table: "ShowcaseCars",
                column: "PriceRub");

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
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CarPhotos");

            migrationBuilder.DropTable(
                name: "CarSeries");

            migrationBuilder.DropTable(
                name: "ClientCards");

            migrationBuilder.DropTable(
                name: "ContentSectionItems");

            migrationBuilder.DropTable(
                name: "ContentSections");

            migrationBuilder.DropTable(
                name: "CurrencyRates");

            migrationBuilder.DropTable(
                name: "FaqItems");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "Offices");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "SearchResultItems");

            migrationBuilder.DropTable(
                name: "ServiceTariffs");

            migrationBuilder.DropTable(
                name: "ShowcaseCarPhotos");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "CarListings");

            migrationBuilder.DropTable(
                name: "SearchRequests");

            migrationBuilder.DropTable(
                name: "ShowcaseCars");

            migrationBuilder.DropTable(
                name: "SourceSites");
        }
    }
}
