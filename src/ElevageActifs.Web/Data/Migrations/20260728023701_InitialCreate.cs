using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElevageActifs.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "elevageactifs");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exploitations",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Province = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    TotalAreaHa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductionType = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exploitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionDefinitions",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Resource = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PropertyName = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionDefinitions", x => x.Id);
                    table.UniqueConstraint("AK_PermissionDefinitions_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ThemeDefinitions",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CssVariables = table.Column<string>(type: "text", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "elevageactifs",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "elevageactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "elevageactifs",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "elevageactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "elevageactifs",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "elevageactifs",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "elevageactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "elevageactifs",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "elevageactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    Company = table.Column<string>(type: "text", nullable: true),
                    JobTitle = table.Column<string>(type: "text", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "text", nullable: false),
                    Theme = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    EmailNotifications = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "elevageactifs",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enclos",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enclos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enclos_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExploitationUsers",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExploitationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExploitationUsers_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fournisseurs_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolesSanitaires",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Species = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolesSanitaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocolesSanitaires_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockArticles",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Categorie = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LotNumber = table.Column<string>(type: "text", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockArticles_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Troupeaux",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Species = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Troupeaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Troupeaux_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportDefinitions",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RequiredPermissionCode = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportDefinitions_PermissionDefinitions_RequiredPermissionC~",
                        column: x => x.RequiredPermissionCode,
                        principalSchema: "elevageactifs",
                        principalTable: "PermissionDefinitions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissionGrants",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    PermissionDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissionGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissionGrants_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "elevageactifs",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissionGrants_PermissionDefinitions_PermissionDefini~",
                        column: x => x.PermissionDefinitionId,
                        principalSchema: "elevageactifs",
                        principalTable: "PermissionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecuredEndpoints",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Area = table.Column<string>(type: "text", nullable: true),
                    Controller = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    HttpMethod = table.Column<string>(type: "text", nullable: true),
                    PermissionDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecuredEndpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecuredEndpoints_PermissionDefinitions_PermissionDefinition~",
                        column: x => x.PermissionDefinitionId,
                        principalSchema: "elevageactifs",
                        principalTable: "PermissionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppName = table.Column<string>(type: "text", nullable: false),
                    Tagline = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    ActiveThemeId = table.Column<int>(type: "integer", nullable: false),
                    DefaultCulture = table.Column<string>(type: "text", nullable: false),
                    SmtpHost = table.Column<string>(type: "text", nullable: true),
                    SmtpPort = table.Column<int>(type: "integer", nullable: false),
                    SmtpUser = table.Column<string>(type: "text", nullable: true),
                    SmtpUseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    RequireConfirmedEmail = table.Column<bool>(type: "boolean", nullable: false),
                    RequireTwoFactor = table.Column<bool>(type: "boolean", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxFailedAccessAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSettings_ThemeDefinitions_ActiveThemeId",
                        column: x => x.ActiveThemeId,
                        principalSchema: "elevageactifs",
                        principalTable: "ThemeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActifsMateriel",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    InternalCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Categorie = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcquisitionValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UsefulLifeYears = table.Column<int>(type: "integer", nullable: true),
                    ResidualValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EnclosId = table.Column<int>(type: "integer", nullable: true),
                    LocationNote = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActifsMateriel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActifsMateriel_Enclos_EnclosId",
                        column: x => x.EnclosId,
                        principalSchema: "elevageactifs",
                        principalTable: "Enclos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActifsMateriel_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMouvements",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockArticleId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    MovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMouvements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMouvements_StockArticles_StockArticleId",
                        column: x => x.StockArticleId,
                        principalSchema: "elevageactifs",
                        principalTable: "StockArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lots",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TroupeauId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lots_Troupeaux_TroupeauId",
                        column: x => x.TroupeauId,
                        principalSchema: "elevageactifs",
                        principalTable: "Troupeaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interventions",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    ActifMaterielId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LaborCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PartsCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Report = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interventions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interventions_ActifsMateriel_ActifMaterielId",
                        column: x => x.ActifMaterielId,
                        principalSchema: "elevageactifs",
                        principalTable: "ActifsMateriel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Interventions_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Animaux",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    BoucleNumber = table.Column<string>(type: "text", nullable: false),
                    RfidTag = table.Column<string>(type: "text", nullable: true),
                    Species = table.Column<string>(type: "text", nullable: false),
                    Race = table.Column<string>(type: "text", nullable: true),
                    Sex = table.Column<string>(type: "text", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    MotherId = table.Column<int>(type: "integer", nullable: true),
                    FatherId = table.Column<int>(type: "integer", nullable: true),
                    TroupeauId = table.Column<int>(type: "integer", nullable: true),
                    LotId = table.Column<int>(type: "integer", nullable: true),
                    EnclosId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Animaux_Animaux_FatherId",
                        column: x => x.FatherId,
                        principalSchema: "elevageactifs",
                        principalTable: "Animaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Animaux_Animaux_MotherId",
                        column: x => x.MotherId,
                        principalSchema: "elevageactifs",
                        principalTable: "Animaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Animaux_Enclos_EnclosId",
                        column: x => x.EnclosId,
                        principalSchema: "elevageactifs",
                        principalTable: "Enclos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Animaux_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Animaux_Lots_LotId",
                        column: x => x.LotId,
                        principalSchema: "elevageactifs",
                        principalTable: "Lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Animaux_Troupeaux_TroupeauId",
                        column: x => x.TroupeauId,
                        principalSchema: "elevageactifs",
                        principalTable: "Troupeaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AnimalEvenements",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnimalId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalEvenements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalEvenements_Animaux_AnimalId",
                        column: x => x.AnimalId,
                        principalSchema: "elevageactifs",
                        principalTable: "Animaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvenementsReproduction",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    AnimalId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvenementsReproduction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvenementsReproduction_Animaux_AnimalId",
                        column: x => x.AnimalId,
                        principalSchema: "elevageactifs",
                        principalTable: "Animaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvenementsReproduction_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Traitements",
                schema: "elevageactifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExploitationId = table.Column<int>(type: "integer", nullable: false),
                    AnimalId = table.Column<int>(type: "integer", nullable: true),
                    LotId = table.Column<int>(type: "integer", nullable: true),
                    ProtocoleSanitaireId = table.Column<int>(type: "integer", nullable: true),
                    Product = table.Column<string>(type: "text", nullable: false),
                    Dose = table.Column<string>(type: "text", nullable: true),
                    AdministeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WaitMilkUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaitMeatUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traitements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Traitements_Animaux_AnimalId",
                        column: x => x.AnimalId,
                        principalSchema: "elevageactifs",
                        principalTable: "Animaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Traitements_Exploitations_ExploitationId",
                        column: x => x.ExploitationId,
                        principalSchema: "elevageactifs",
                        principalTable: "Exploitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Traitements_Lots_LotId",
                        column: x => x.LotId,
                        principalSchema: "elevageactifs",
                        principalTable: "Lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Traitements_ProtocolesSanitaires_ProtocoleSanitaireId",
                        column: x => x.ProtocoleSanitaireId,
                        principalSchema: "elevageactifs",
                        principalTable: "ProtocolesSanitaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "elevageactifs",
                table: "ThemeDefinitions",
                columns: new[] { "Id", "Code", "CreatedAt", "CssVariables", "Description", "IsActive", "IsSystem", "Name" },
                values: new object[,]
                {
                    { 1, "default", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"--gise-primary\": \"#1e40af\",\"--gise-primary-dark\": \"#1e3a8a\",\"--gise-accent\": \"#0ea5e9\",\"--gise-accent-soft\": \"#e0f2fe\",\"--gise-success\": \"#059669\",\"--gise-warning\": \"#d97706\",\"--gise-danger\": \"#dc2626\",\"--gise-sidebar\": \"#0f172a\",\"--gise-sidebar-hover\": \"#1e293b\",\"--gise-sidebar-active\": \"#2563eb\",\"--gise-surface\": \"#ffffff\",\"--gise-bg\": \"#f1f5f9\",\"--gise-border\": \"#e2e8f0\",\"--gise-text\": \"#0f172a\",\"--gise-text-muted\": \"#64748b\"}", "Palette bleue d'origine", true, true, "GISEBS Default" },
                    { 2, "corporate", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"--gise-primary\": \"#374151\",\"--gise-primary-dark\": \"#1f2937\",\"--gise-accent\": \"#6b7280\",\"--gise-accent-soft\": \"#f3f4f6\",\"--gise-success\": \"#059669\",\"--gise-warning\": \"#d97706\",\"--gise-danger\": \"#dc2626\",\"--gise-sidebar\": \"#111827\",\"--gise-sidebar-hover\": \"#1f2937\",\"--gise-sidebar-active\": \"#4b5563\",\"--gise-surface\": \"#ffffff\",\"--gise-bg\": \"#f9fafb\",\"--gise-border\": \"#e5e7eb\",\"--gise-text\": \"#111827\",\"--gise-text-muted\": \"#6b7280\"}", "Tons neutres professionnels", true, true, "Corporate" },
                    { 3, "ocean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"--gise-primary\": \"#0d9488\",\"--gise-primary-dark\": \"#0f766e\",\"--gise-accent\": \"#06b6d4\",\"--gise-accent-soft\": \"#cffafe\",\"--gise-success\": \"#059669\",\"--gise-warning\": \"#d97706\",\"--gise-danger\": \"#dc2626\",\"--gise-sidebar\": \"#134e4a\",\"--gise-sidebar-hover\": \"#115e59\",\"--gise-sidebar-active\": \"#0d9488\",\"--gise-surface\": \"#ffffff\",\"--gise-bg\": \"#f0fdfa\",\"--gise-border\": \"#ccfbf1\",\"--gise-text\": \"#134e4a\",\"--gise-text-muted\": \"#5eead4\"}", "Bleu-vert moderne", true, true, "Ocean" }
                });

            migrationBuilder.InsertData(
                schema: "elevageactifs",
                table: "SystemSettings",
                columns: new[] { "Id", "ActiveThemeId", "AppName", "DefaultCulture", "LockoutMinutes", "LogoUrl", "MaxFailedAccessAttempts", "RequireConfirmedEmail", "RequireTwoFactor", "SessionTimeoutMinutes", "SmtpHost", "SmtpPort", "SmtpUseSsl", "SmtpUser", "Tagline", "UpdatedAt" },
                values: new object[] { 1, 1, "GISEBS Secure MVC Starter", "fr-FR", 15, null, 5, true, false, 30, null, 587, true, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_ActifsMateriel_EnclosId",
                schema: "elevageactifs",
                table: "ActifsMateriel",
                column: "EnclosId");

            migrationBuilder.CreateIndex(
                name: "IX_ActifsMateriel_ExploitationId_InternalCode",
                schema: "elevageactifs",
                table: "ActifsMateriel",
                columns: new[] { "ExploitationId", "InternalCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalEvenements_AnimalId",
                schema: "elevageactifs",
                table: "AnimalEvenements",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_Animaux_EnclosId",
                schema: "elevageactifs",
                table: "Animaux",
                column: "EnclosId");

            migrationBuilder.CreateIndex(
                name: "IX_Animaux_ExploitationId_BoucleNumber",
                schema: "elevageactifs",
                table: "Animaux",
                columns: new[] { "ExploitationId", "BoucleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Animaux_FatherId",
                schema: "elevageactifs",
                table: "Animaux",
                column: "FatherId");

            migrationBuilder.CreateIndex(
                name: "IX_Animaux_LotId",
                schema: "elevageactifs",
                table: "Animaux",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_Animaux_MotherId",
                schema: "elevageactifs",
                table: "Animaux",
                column: "MotherId");

            migrationBuilder.CreateIndex(
                name: "IX_Animaux_TroupeauId",
                schema: "elevageactifs",
                table: "Animaux",
                column: "TroupeauId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "elevageactifs",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "elevageactifs",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "elevageactifs",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "elevageactifs",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "elevageactifs",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "elevageactifs",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "elevageactifs",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                schema: "elevageactifs",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "elevageactifs",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Enclos_ExploitationId_Code",
                schema: "elevageactifs",
                table: "Enclos",
                columns: new[] { "ExploitationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvenementsReproduction_AnimalId",
                schema: "elevageactifs",
                table: "EvenementsReproduction",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_EvenementsReproduction_ExploitationId",
                schema: "elevageactifs",
                table: "EvenementsReproduction",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExploitationUsers_ExploitationId_UserId",
                schema: "elevageactifs",
                table: "ExploitationUsers",
                columns: new[] { "ExploitationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseurs_ExploitationId",
                schema: "elevageactifs",
                table: "Fournisseurs",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_ActifMaterielId",
                schema: "elevageactifs",
                table: "Interventions",
                column: "ActifMaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_ExploitationId",
                schema: "elevageactifs",
                table: "Interventions",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_Lots_TroupeauId_Code",
                schema: "elevageactifs",
                table: "Lots",
                columns: new[] { "TroupeauId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDefinitions_Code",
                schema: "elevageactifs",
                table: "PermissionDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionDefinitions_Resource_Action_PropertyName",
                schema: "elevageactifs",
                table: "PermissionDefinitions",
                columns: new[] { "Resource", "Action", "PropertyName" });

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolesSanitaires_ExploitationId",
                schema: "elevageactifs",
                table: "ProtocolesSanitaires",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_Code",
                schema: "elevageactifs",
                table: "ReportDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_RequiredPermissionCode",
                schema: "elevageactifs",
                table: "ReportDefinitions",
                column: "RequiredPermissionCode");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionGrants_PermissionDefinitionId",
                schema: "elevageactifs",
                table: "RolePermissionGrants",
                column: "PermissionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionGrants_RoleId_PermissionDefinitionId",
                schema: "elevageactifs",
                table: "RolePermissionGrants",
                columns: new[] { "RoleId", "PermissionDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecuredEndpoints_Area_Controller_Action_HttpMethod",
                schema: "elevageactifs",
                table: "SecuredEndpoints",
                columns: new[] { "Area", "Controller", "Action", "HttpMethod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecuredEndpoints_PermissionDefinitionId",
                schema: "elevageactifs",
                table: "SecuredEndpoints",
                column: "PermissionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockArticles_ExploitationId_Sku",
                schema: "elevageactifs",
                table: "StockArticles",
                columns: new[] { "ExploitationId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMouvements_StockArticleId",
                schema: "elevageactifs",
                table: "StockMouvements",
                column: "StockArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_ActiveThemeId",
                schema: "elevageactifs",
                table: "SystemSettings",
                column: "ActiveThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ThemeDefinitions_Code",
                schema: "elevageactifs",
                table: "ThemeDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Traitements_AnimalId",
                schema: "elevageactifs",
                table: "Traitements",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_Traitements_ExploitationId",
                schema: "elevageactifs",
                table: "Traitements",
                column: "ExploitationId");

            migrationBuilder.CreateIndex(
                name: "IX_Traitements_LotId",
                schema: "elevageactifs",
                table: "Traitements",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_Traitements_ProtocoleSanitaireId",
                schema: "elevageactifs",
                table: "Traitements",
                column: "ProtocoleSanitaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Troupeaux_ExploitationId_Code",
                schema: "elevageactifs",
                table: "Troupeaux",
                columns: new[] { "ExploitationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                schema: "elevageactifs",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalEvenements",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "EvenementsReproduction",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "ExploitationUsers",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "Fournisseurs",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "Interventions",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "ReportDefinitions",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "RolePermissionGrants",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "SecuredEndpoints",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "StockMouvements",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "SystemSettings",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "Traitements",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "UserProfiles",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "ActifsMateriel",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "PermissionDefinitions",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "StockArticles",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "ThemeDefinitions",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "Animaux",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "ProtocolesSanitaires",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "Enclos",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "Lots",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "Troupeaux",
                schema: "elevageactifs");

            migrationBuilder.DropTable(
                name: "Exploitations",
                schema: "elevageactifs");
        }
    }
}
