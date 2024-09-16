using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class steam_string : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SteamId",
                table: "Members",
                type: "text",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SteamId",
                table: "Members",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
