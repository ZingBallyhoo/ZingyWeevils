using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinWeevils.Common.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apparelTypes",
                columns: table => new
                {
                    id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    category = table.Column<byte>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    paletteId = table.Column<uint>(type: "INTEGER", nullable: false),
                    price = table.Column<uint>(type: "INTEGER", nullable: false),
                    probability = table.Column<byte>(type: "INTEGER", nullable: false),
                    minLevel = table.Column<byte>(type: "INTEGER", nullable: false),
                    tycoonOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apparelTypes", x => x.id);
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
                name: "itemType",
                columns: table => new
                {
                    itemTypeID = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    category = table.Column<int>(type: "INTEGER", nullable: false),
                    configLocation = table.Column<string>(type: "TEXT", nullable: false),
                    shopType = table.Column<string>(type: "TEXT", nullable: false),
                    paletteID = table.Column<int>(type: "INTEGER", nullable: false),
                    defaultHexcolour = table.Column<string>(type: "TEXT", nullable: false),
                    currency = table.Column<string>(type: "TEXT", nullable: false),
                    price = table.Column<int>(type: "INTEGER", nullable: false),
                    previousCurrency = table.Column<string>(type: "TEXT", nullable: false),
                    previousPrice = table.Column<int>(type: "INTEGER", nullable: false),
                    probability = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    deliveryTime = table.Column<int>(type: "INTEGER", nullable: false),
                    expPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    powerConsumption = table.Column<int>(type: "INTEGER", nullable: false),
                    boundRadius = table.Column<byte>(type: "INTEGER", nullable: false),
                    collectionID = table.Column<int>(type: "INTEGER", nullable: true),
                    minLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    tycoonOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    canDelete = table.Column<bool>(type: "INTEGER", nullable: false),
                    canGroup = table.Column<bool>(type: "INTEGER", nullable: false),
                    isLive = table.Column<bool>(type: "INTEGER", nullable: false),
                    internalCategory = table.Column<string>(type: "TEXT", nullable: false),
                    coolness = table.Column<int>(type: "INTEGER", nullable: false),
                    ordering = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itemType", x => x.itemTypeID);
                });

            migrationBuilder.CreateTable(
                name: "JugglingTrickDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_numBalls = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_name = table.Column<string>(type: "TEXT", nullable: false),
                    m_difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_pattern = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JugglingTrickDB", x => x.m_id);
                });

            migrationBuilder.CreateTable(
                name: "NestDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_gardenSize = table.Column<int>(type: "INTEGER", nullable: false),
                    m_fuel = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_lastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    m_itemsLastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestDB", x => x.m_id);
                });

            migrationBuilder.CreateTable(
                name: "PaletteEntryDB",
                columns: table => new
                {
                    m_paletteID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_index = table.Column<int>(type: "INTEGER", nullable: false),
                    m_colorString = table.Column<string>(type: "TEXT", nullable: false),
                    m_color_m_b = table.Column<short>(type: "INTEGER", nullable: false),
                    m_color_m_g = table.Column<short>(type: "INTEGER", nullable: false),
                    m_color_m_r = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaletteEntryDB", x => new { x.m_paletteID, x.m_index });
                });

            migrationBuilder.CreateTable(
                name: "seeds",
                columns: table => new
                {
                    id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    category = table.Column<int>(type: "INTEGER", nullable: false),
                    tycoon = table.Column<bool>(type: "INTEGER", nullable: false),
                    level = table.Column<byte>(type: "INTEGER", nullable: false),
                    fileName = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    price = table.Column<uint>(type: "INTEGER", nullable: false),
                    mulchYield = table.Column<uint>(type: "INTEGER", nullable: false),
                    xpYield = table.Column<uint>(type: "INTEGER", nullable: false),
                    growTime = table.Column<uint>(type: "INTEGER", nullable: false),
                    cycleTime = table.Column<uint>(type: "INTEGER", nullable: false),
                    probability = table.Column<byte>(type: "INTEGER", nullable: false),
                    radius = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seeds", x => x.id);
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
                name: "NestGardenItemDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_nestID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_itemTypeID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_color_m_b = table.Column<short>(type: "INTEGER", nullable: false),
                    m_color_m_g = table.Column<short>(type: "INTEGER", nullable: false),
                    m_color_m_r = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestGardenItemDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_NestGardenItemDB_NestDB_m_nestID",
                        column: x => x.m_nestID,
                        principalTable: "NestDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NestGardenItemDB_itemType_m_itemTypeID",
                        column: x => x.m_itemTypeID,
                        principalTable: "itemType",
                        principalColumn: "itemTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NestItemDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_nestID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_itemTypeID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_color_m_b = table.Column<short>(type: "INTEGER", nullable: false),
                    m_color_m_g = table.Column<short>(type: "INTEGER", nullable: false),
                    m_color_m_r = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestItemDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_NestItemDB_NestDB_m_nestID",
                        column: x => x.m_nestID,
                        principalTable: "NestDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NestItemDB_itemType_m_itemTypeID",
                        column: x => x.m_itemTypeID,
                        principalTable: "itemType",
                        principalColumn: "itemTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NestRoomDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_type = table.Column<int>(type: "INTEGER", nullable: false),
                    m_nestID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_color_m_b = table.Column<sbyte>(type: "INTEGER", nullable: false),
                    m_color_m_g = table.Column<sbyte>(type: "INTEGER", nullable: false),
                    m_color_m_r = table.Column<sbyte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestRoomDB", x => x.m_id);
                    table.UniqueConstraint("AK_NestRoomDB_m_nestID_m_type", x => new { x.m_nestID, x.m_type });
                    table.ForeignKey(
                        name: "FK_NestRoomDB_NestDB_m_nestID",
                        column: x => x.m_nestID,
                        principalTable: "NestDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeevilDB",
                columns: table => new
                {
                    m_idx = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_name = table.Column<string>(type: "TEXT", nullable: false),
                    m_createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    m_lastLogin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    m_weevilDef = table.Column<ulong>(type: "INTEGER", nullable: false),
                    m_apparelTypeID = table.Column<uint>(type: "INTEGER", nullable: true),
                    m_apparelPaletteEntryIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    m_introProgress = table.Column<ushort>(type: "INTEGER", nullable: false),
                    m_xp = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_lastAcknowledgedLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    m_food = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_fitness = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_happiness = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_mulch = table.Column<int>(type: "INTEGER", nullable: false),
                    m_dosh = table.Column<int>(type: "INTEGER", nullable: false),
                    m_petFoodStock = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_nestm_id = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeevilDB", x => x.m_idx);
                    table.ForeignKey(
                        name: "FK_WeevilDB_NestDB_m_nestm_id",
                        column: x => x.m_nestm_id,
                        principalTable: "NestDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeevilDB_apparelTypes_m_apparelTypeID",
                        column: x => x.m_apparelTypeID,
                        principalTable: "apparelTypes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "NestSeedItemDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_nestID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_seedTypeID = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestSeedItemDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_NestSeedItemDB_NestDB_m_nestID",
                        column: x => x.m_nestID,
                        principalTable: "NestDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NestSeedItemDB_seeds_m_seedTypeID",
                        column: x => x.m_seedTypeID,
                        principalTable: "seeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_name = table.Column<string>(type: "TEXT", nullable: false),
                    m_open = table.Column<bool>(type: "INTEGER", nullable: false),
                    m_type = table.Column<int>(type: "INTEGER", nullable: false),
                    m_signColor = table.Column<int>(type: "INTEGER", nullable: false),
                    m_signTextColor = table.Column<int>(type: "INTEGER", nullable: false),
                    m_playList_m_track1 = table.Column<short>(type: "INTEGER", nullable: false),
                    m_playList_m_track2 = table.Column<short>(type: "INTEGER", nullable: false),
                    m_playList_m_track3 = table.Column<short>(type: "INTEGER", nullable: false),
                    m_playList_m_track4 = table.Column<short>(type: "INTEGER", nullable: false),
                    m_playList_m_track5 = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_BusinessDB_NestRoomDB_m_id",
                        column: x => x.m_id,
                        principalTable: "NestRoomDB",
                        principalColumn: "m_id");
                });

            migrationBuilder.CreateTable(
                name: "NestPlacedGardenItemDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_roomID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_x = table.Column<short>(type: "INTEGER", nullable: false),
                    m_z = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestPlacedGardenItemDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_NestPlacedGardenItemDB_NestGardenItemDB_m_id",
                        column: x => x.m_id,
                        principalTable: "NestGardenItemDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NestPlacedGardenItemDB_NestRoomDB_m_roomID",
                        column: x => x.m_roomID,
                        principalTable: "NestRoomDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NestPlacedItemDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_roomID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_posAnimationFrame = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_placedOnFurnitureID = table.Column<uint>(type: "INTEGER", nullable: true),
                    m_posIdentity = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_spotOnFurniture = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestPlacedItemDB", x => x.m_id);
                    table.UniqueConstraint("AK_NestPlacedItemDB_m_roomID_m_posIdentity_m_spotOnFurniture_m_posAnimationFrame", x => new { x.m_roomID, x.m_posIdentity, x.m_spotOnFurniture, x.m_posAnimationFrame });
                    table.ForeignKey(
                        name: "FK_NestPlacedItemDB_NestItemDB_m_id",
                        column: x => x.m_id,
                        principalTable: "NestItemDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NestPlacedItemDB_NestPlacedItemDB_m_placedOnFurnitureID",
                        column: x => x.m_placedOnFurnitureID,
                        principalTable: "NestPlacedItemDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NestPlacedItemDB_NestRoomDB_m_roomID",
                        column: x => x.m_roomID,
                        principalTable: "NestRoomDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    m_weevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_AspNetUsers_WeevilDB_m_weevilIdx",
                        column: x => x.m_weevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuddyMessageDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_to = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_from = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_message = table.Column<string>(type: "TEXT", nullable: false),
                    m_sentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    m_read = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuddyMessageDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_BuddyMessageDB_WeevilDB_m_from",
                        column: x => x.m_from,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuddyMessageDB_WeevilDB_m_to",
                        column: x => x.m_to,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuddyRecordDB",
                columns: table => new
                {
                    m_weevil1ID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_weevil2ID = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuddyRecordDB", x => new { x.m_weevil1ID, x.m_weevil2ID });
                    table.ForeignKey(
                        name: "FK_BuddyRecordDB_WeevilDB_m_weevil1ID",
                        column: x => x.m_weevil1ID,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuddyRecordDB_WeevilDB_m_weevil2ID",
                        column: x => x.m_weevil2ID,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletedTaskDB",
                columns: table => new
                {
                    m_weevilID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_taskID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedTaskDB", x => new { x.m_weevilID, x.m_taskID });
                    table.ForeignKey(
                        name: "FK_CompletedTaskDB_WeevilDB_m_weevilID",
                        column: x => x.m_weevilID,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IgnoreRecordDB",
                columns: table => new
                {
                    m_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_forWeevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_ignoredWeevilIdx = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgnoreRecordDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_IgnoreRecordDB_WeevilDB_m_forWeevilIdx",
                        column: x => x.m_forWeevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IgnoreRecordDB_WeevilDB_m_ignoredWeevilIdx",
                        column: x => x.m_ignoredWeevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    m_ownerIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_adoptedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    m_name = table.Column<string>(type: "TEXT", nullable: false),
                    m_bodyColor = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_antenna1Color = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_antenna2Color = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_eye1Color = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_eye2Color = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_mentalEnergy = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_fuel = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_health = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_fitness = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_experience = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_bedItemm_id = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_bowlItemm_id = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_PetDB_NestItemDB_m_bedItemm_id",
                        column: x => x.m_bedItemm_id,
                        principalTable: "NestItemDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PetDB_NestItemDB_m_bowlItemm_id",
                        column: x => x.m_bowlItemm_id,
                        principalTable: "NestItemDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PetDB_WeevilDB_m_ownerIdx",
                        column: x => x.m_ownerIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardedTaskDB",
                columns: table => new
                {
                    m_weevilID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_taskID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardedTaskDB", x => new { x.m_weevilID, x.m_taskID });
                    table.ForeignKey(
                        name: "FK_RewardedTaskDB_WeevilDB_m_weevilID",
                        column: x => x.m_weevilID,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeevilGamePlayedDB",
                columns: table => new
                {
                    m_weevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_gameType = table.Column<int>(type: "INTEGER", nullable: false),
                    m_lastPlayed = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeevilGamePlayedDB", x => new { x.m_weevilIdx, x.m_gameType });
                    table.ForeignKey(
                        name: "FK_WeevilGamePlayedDB_WeevilDB_m_weevilIdx",
                        column: x => x.m_weevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeevilSpecialMoveDB",
                columns: table => new
                {
                    m_weevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_action = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeevilSpecialMoveDB", x => new { x.m_weevilIdx, x.m_action });
                    table.ForeignKey(
                        name: "FK_WeevilSpecialMoveDB_WeevilDB_m_weevilIdx",
                        column: x => x.m_weevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NestPlantDB",
                columns: table => new
                {
                    m_id = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_x = table.Column<short>(type: "INTEGER", nullable: false),
                    m_z = table.Column<short>(type: "INTEGER", nullable: false),
                    m_growthStartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NestPlantDB", x => x.m_id);
                    table.ForeignKey(
                        name: "FK_NestPlantDB_NestSeedItemDB_m_id",
                        column: x => x.m_id,
                        principalTable: "NestSeedItemDB",
                        principalColumn: "m_id",
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
                name: "PetJugglingTrickDB",
                columns: table => new
                {
                    m_petID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_trickID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_aptitude = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetJugglingTrickDB", x => new { x.m_petID, x.m_trickID });
                    table.ForeignKey(
                        name: "FK_PetJugglingTrickDB_JugglingTrickDB_m_trickID",
                        column: x => x.m_trickID,
                        principalTable: "JugglingTrickDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PetJugglingTrickDB_PetDB_m_petID",
                        column: x => x.m_petID,
                        principalTable: "PetDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetSkillDB",
                columns: table => new
                {
                    m_petID = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_skillID = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_obedience = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_skillLevel = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetSkillDB", x => new { x.m_petID, x.m_skillID });
                    table.ForeignKey(
                        name: "FK_PetSkillDB_PetDB_m_petID",
                        column: x => x.m_petID,
                        principalTable: "PetDB",
                        principalColumn: "m_id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_AspNetUsers_m_weevilIdx",
                table: "AspNetUsers",
                column: "m_weevilIdx");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuddyMessageDB_m_from",
                table: "BuddyMessageDB",
                column: "m_from");

            migrationBuilder.CreateIndex(
                name: "IX_BuddyMessageDB_m_to",
                table: "BuddyMessageDB",
                column: "m_to");

            migrationBuilder.CreateIndex(
                name: "IX_BuddyRecordDB_m_weevil2ID",
                table: "BuddyRecordDB",
                column: "m_weevil2ID");

            migrationBuilder.CreateIndex(
                name: "IX_IgnoreRecordDB_m_forWeevilIdx",
                table: "IgnoreRecordDB",
                column: "m_forWeevilIdx");

            migrationBuilder.CreateIndex(
                name: "IX_IgnoreRecordDB_m_ignoredWeevilIdx",
                table: "IgnoreRecordDB",
                column: "m_ignoredWeevilIdx");

            migrationBuilder.CreateIndex(
                name: "IX_itemType_configLocation",
                table: "itemType",
                column: "configLocation");

            migrationBuilder.CreateIndex(
                name: "IX_JugglingTrickDB_m_pattern",
                table: "JugglingTrickDB",
                column: "m_pattern");

            migrationBuilder.CreateIndex(
                name: "IX_NestGardenItemDB_m_itemTypeID",
                table: "NestGardenItemDB",
                column: "m_itemTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_NestGardenItemDB_m_nestID",
                table: "NestGardenItemDB",
                column: "m_nestID");

            migrationBuilder.CreateIndex(
                name: "IX_NestItemDB_m_itemTypeID",
                table: "NestItemDB",
                column: "m_itemTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_NestItemDB_m_nestID",
                table: "NestItemDB",
                column: "m_nestID");

            migrationBuilder.CreateIndex(
                name: "IX_NestPlacedGardenItemDB_m_roomID",
                table: "NestPlacedGardenItemDB",
                column: "m_roomID");

            migrationBuilder.CreateIndex(
                name: "IX_NestPlacedItemDB_m_placedOnFurnitureID",
                table: "NestPlacedItemDB",
                column: "m_placedOnFurnitureID");

            migrationBuilder.CreateIndex(
                name: "IX_NestSeedItemDB_m_nestID",
                table: "NestSeedItemDB",
                column: "m_nestID");

            migrationBuilder.CreateIndex(
                name: "IX_NestSeedItemDB_m_seedTypeID",
                table: "NestSeedItemDB",
                column: "m_seedTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_PaletteEntryDB_m_paletteID_m_colorString",
                table: "PaletteEntryDB",
                columns: new[] { "m_paletteID", "m_colorString" });

            migrationBuilder.CreateIndex(
                name: "IX_PetDB_m_bedItemm_id",
                table: "PetDB",
                column: "m_bedItemm_id");

            migrationBuilder.CreateIndex(
                name: "IX_PetDB_m_bowlItemm_id",
                table: "PetDB",
                column: "m_bowlItemm_id");

            migrationBuilder.CreateIndex(
                name: "IX_PetDB_m_ownerIdx",
                table: "PetDB",
                column: "m_ownerIdx");

            migrationBuilder.CreateIndex(
                name: "IX_PetJugglingTrickDB_m_trickID",
                table: "PetJugglingTrickDB",
                column: "m_trickID");

            migrationBuilder.CreateIndex(
                name: "IX_WeevilDB_m_apparelTypeID",
                table: "WeevilDB",
                column: "m_apparelTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_WeevilDB_m_name",
                table: "WeevilDB",
                column: "m_name");

            migrationBuilder.CreateIndex(
                name: "IX_WeevilDB_m_nestm_id",
                table: "WeevilDB",
                column: "m_nestm_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "BuddyMessageDB");

            migrationBuilder.DropTable(
                name: "BuddyRecordDB");

            migrationBuilder.DropTable(
                name: "BusinessDB");

            migrationBuilder.DropTable(
                name: "CompletedTaskDB");

            migrationBuilder.DropTable(
                name: "IgnoreRecordDB");

            migrationBuilder.DropTable(
                name: "NestPlacedGardenItemDB");

            migrationBuilder.DropTable(
                name: "NestPlacedItemDB");

            migrationBuilder.DropTable(
                name: "NestPlantDB");

            migrationBuilder.DropTable(
                name: "PaletteEntryDB");

            migrationBuilder.DropTable(
                name: "PetJugglingTrickDB");

            migrationBuilder.DropTable(
                name: "PetSkillDB");

            migrationBuilder.DropTable(
                name: "RewardedTaskDB");

            migrationBuilder.DropTable(
                name: "WeevilGamePlayedDB");

            migrationBuilder.DropTable(
                name: "WeevilSpecialMoveDB");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "NestGardenItemDB");

            migrationBuilder.DropTable(
                name: "NestRoomDB");

            migrationBuilder.DropTable(
                name: "NestSeedItemDB");

            migrationBuilder.DropTable(
                name: "JugglingTrickDB");

            migrationBuilder.DropTable(
                name: "PetDB");

            migrationBuilder.DropTable(
                name: "seeds");

            migrationBuilder.DropTable(
                name: "NestItemDB");

            migrationBuilder.DropTable(
                name: "WeevilDB");

            migrationBuilder.DropTable(
                name: "itemType");

            migrationBuilder.DropTable(
                name: "NestDB");

            migrationBuilder.DropTable(
                name: "apparelTypes");
        }
    }
}
