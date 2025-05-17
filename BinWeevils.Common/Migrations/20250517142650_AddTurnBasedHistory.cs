using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinWeevils.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnBasedHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeevilTurnBasedGamePlayedDB",
                columns: table => new
                {
                    m_weevilIdx = table.Column<uint>(type: "INTEGER", nullable: false),
                    m_gameType = table.Column<int>(type: "INTEGER", nullable: false),
                    m_lastPlayed = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeevilTurnBasedGamePlayedDB", x => new { x.m_weevilIdx, x.m_gameType });
                    table.ForeignKey(
                        name: "FK_WeevilTurnBasedGamePlayedDB_WeevilDB_m_weevilIdx",
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
                name: "WeevilTurnBasedGamePlayedDB");
        }
    }
}
