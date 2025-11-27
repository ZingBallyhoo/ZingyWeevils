using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinWeevils.Common.Migrations
{
    /// <inheritdoc />
    public partial class Net10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_r",
                table: "PaletteEntryDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_g",
                table: "PaletteEntryDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_b",
                table: "PaletteEntryDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<sbyte>(
                name: "m_color_m_r",
                table: "NestRoomDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<sbyte>(
                name: "m_color_m_g",
                table: "NestRoomDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<sbyte>(
                name: "m_color_m_b",
                table: "NestRoomDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_r",
                table: "NestItemDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_g",
                table: "NestItemDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_b",
                table: "NestItemDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_r",
                table: "NestGardenItemDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_g",
                table: "NestGardenItemDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_b",
                table: "NestGardenItemDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track5",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track4",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track3",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track2",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track1",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_r",
                table: "PaletteEntryDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_g",
                table: "PaletteEntryDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_b",
                table: "PaletteEntryDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<sbyte>(
                name: "m_color_m_r",
                table: "NestRoomDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(sbyte),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<sbyte>(
                name: "m_color_m_g",
                table: "NestRoomDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(sbyte),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<sbyte>(
                name: "m_color_m_b",
                table: "NestRoomDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(sbyte),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_r",
                table: "NestItemDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_g",
                table: "NestItemDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_b",
                table: "NestItemDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_r",
                table: "NestGardenItemDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_g",
                table: "NestGardenItemDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_color_m_b",
                table: "NestGardenItemDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track5",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track4",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track3",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track2",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<short>(
                name: "m_playList_m_track1",
                table: "BusinessDB",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "INTEGER");
        }
    }
}
