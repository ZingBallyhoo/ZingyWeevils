using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinWeevils.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackPBs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeevilTrackPersonalBestDB",
                columns: table => new
                {
                    m_weevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_gameType = table.Column<int>(type: "INTEGER", nullable: false),
                    m_lap1 = table.Column<double>(type: "REAL", nullable: false),
                    m_lap2 = table.Column<double>(type: "REAL", nullable: false),
                    m_lap3 = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeevilTrackPersonalBestDB", x => new { x.m_weevilIdx, x.m_gameType });
                    table.ForeignKey(
                        name: "FK_WeevilTrackPersonalBestDB_WeevilDB_m_weevilIdx",
                        column: x => x.m_weevilIdx,
                        principalTable: "WeevilDB",
                        principalColumn: "m_idx",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeevilTrackPersonalBestDB");
        }
    }
}
