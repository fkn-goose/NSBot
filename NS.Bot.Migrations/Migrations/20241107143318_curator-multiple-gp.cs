using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class curatormultiplegp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GuildMembers_CuratorId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CuratorId",
                table: "Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Groups_CuratorId",
                table: "Groups",
                column: "CuratorId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GuildMembers_CuratorId",
                table: "Groups",
                column: "CuratorId",
                principalTable: "GuildMembers",
                principalColumn: "Id");
        }
    }
}
