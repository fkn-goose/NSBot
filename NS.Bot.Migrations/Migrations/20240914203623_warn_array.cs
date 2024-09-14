using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class warn_array : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Indefinite",
                table: "Warns",
                newName: "Permanent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Permanent",
                table: "Warns",
                newName: "Indefinite");
        }
    }
}
