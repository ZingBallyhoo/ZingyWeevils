using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinWeevils.Common.Migrations
{
    /// <inheritdoc />
    public partial class Puzzles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeevilCrosswordProgressDB",
                columns: table => new
                {
                    m_weevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_puzzleID = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_progress = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    m_complete = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeevilCrosswordProgressDB", x => new { x.m_weevilIdx, x.m_puzzleID });
                    table.ForeignKey(
                        name: "FK_WeevilCrosswordProgressDB_WeevilDB_m_weevilIdx",
                        column: x => x.m_weevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeevilWordSearchProgressDB",
                columns: table => new
                {
                    m_weevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_puzzleID = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_complete = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeevilWordSearchProgressDB", x => new { x.m_weevilIdx, x.m_puzzleID });
                    table.ForeignKey(
                        name: "FK_WeevilWordSearchProgressDB_WeevilDB_m_weevilIdx",
                        column: x => x.m_weevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeevilWordSearchSpanDB",
                columns: table => new
                {
                    m_weevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_puzzleID = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_iStart = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_jStart = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_iEnd = table.Column<byte>(type: "INTEGER", nullable: false),
                    m_jEnd = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeevilWordSearchSpanDB", x => new { x.m_weevilIdx, x.m_puzzleID, x.m_iStart, x.m_jStart, x.m_iEnd, x.m_jEnd });
                    table.ForeignKey(
                        name: "FK_WeevilWordSearchSpanDB_WeevilDB_m_weevilIdx",
                        column: x => x.m_weevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeevilWordSearchSpanDB_WeevilWordSearchProgressDB_m_weevilIdx_m_puzzleID",
                        columns: x => new { x.m_weevilIdx, x.m_puzzleID },
                        principalTable: "WeevilWordSearchProgressDB",
                        principalColumns: new[] { "m_weevilIdx", "m_puzzleID" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeevilCrosswordProgressDB");

            migrationBuilder.DropTable(
                name: "WeevilWordSearchSpanDB");

            migrationBuilder.DropTable(
                name: "WeevilWordSearchProgressDB");
        }
    }
}
