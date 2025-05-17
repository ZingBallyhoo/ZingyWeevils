using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinWeevils.Common.Migrations
{
    /// <inheritdoc />
    public partial class GameOneTimeAwards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "m_awardGiven",
                table: "WeevilGamePlayedDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "m_awardGiven",
                table: "WeevilGamePlayedDB");
        }
    }
}
