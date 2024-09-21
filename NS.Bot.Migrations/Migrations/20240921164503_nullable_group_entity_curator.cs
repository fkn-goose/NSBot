using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class nullable_group_entity_curator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GuildMembers_CuratorId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Members_MemberId",
                table: "GuildMembers");

            migrationBuilder.AlterColumn<long>(
                name: "MemberId",
                table: "GuildMembers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CuratorId",
                table: "Groups",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GuildMembers_CuratorId",
                table: "Groups",
                column: "CuratorId",
                principalTable: "GuildMembers",
                principalColumn: "Id");

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
                name: "FK_Groups_GuildMembers_CuratorId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Members_MemberId",
                table: "GuildMembers");

            migrationBuilder.AlterColumn<long>(
                name: "MemberId",
                table: "GuildMembers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "CuratorId",
                table: "Groups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GuildMembers_CuratorId",
                table: "Groups",
                column: "CuratorId",
                principalTable: "GuildMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Members_MemberId",
                table: "GuildMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
