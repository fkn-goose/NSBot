using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class admin_roles_additional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Groups_Id",
                table: "GuildMembers");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GuildMembers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "CuratorId",
                table: "Groups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

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
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GuildMembers_CuratorId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CuratorId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CuratorId",
                table: "Groups");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GuildMembers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Groups_Id",
                table: "GuildMembers",
                column: "Id",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
