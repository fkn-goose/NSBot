using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class member_fk_again : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Members_Id",
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
                name: "MemberId",
                table: "GuildMembers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_GuildMembers_MemberId",
                table: "GuildMembers",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Members_MemberId",
                table: "GuildMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Members_MemberId",
                table: "GuildMembers");

            migrationBuilder.DropIndex(
                name: "IX_GuildMembers_MemberId",
                table: "GuildMembers");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "GuildMembers");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GuildMembers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Members_Id",
                table: "GuildMembers",
                column: "Id",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
