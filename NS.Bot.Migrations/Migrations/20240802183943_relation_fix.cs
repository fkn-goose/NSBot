using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class relation_fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GuildMembers_LeaderId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Groups_GroupEntityId",
                table: "GuildMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Members_MemberEntityId",
                table: "GuildMembers");

            migrationBuilder.DropIndex(
                name: "IX_Groups_LeaderId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "LeaderId",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "MemberEntityId",
                table: "GuildMembers",
                newName: "MemberId");

            migrationBuilder.RenameColumn(
                name: "GroupEntityId",
                table: "GuildMembers",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildMembers_MemberEntityId",
                table: "GuildMembers",
                newName: "IX_GuildMembers_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildMembers_GroupEntityId",
                table: "GuildMembers",
                newName: "IX_GuildMembers_GroupId");

            migrationBuilder.RenameColumn(
                name: "Group",
                table: "Groups",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_Group",
                table: "Groups",
                newName: "IX_Groups_Name");

            migrationBuilder.AddColumn<long>(
                name: "Leader",
                table: "Groups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Groups_GroupId",
                table: "GuildMembers",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Members_MemberId",
                table: "GuildMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Groups_GroupId",
                table: "GuildMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Members_MemberId",
                table: "GuildMembers");

            migrationBuilder.DropColumn(
                name: "Leader",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "GuildMembers",
                newName: "MemberEntityId");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "GuildMembers",
                newName: "GroupEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildMembers_MemberId",
                table: "GuildMembers",
                newName: "IX_GuildMembers_MemberEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildMembers_GroupId",
                table: "GuildMembers",
                newName: "IX_GuildMembers_GroupEntityId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Groups",
                newName: "Group");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                newName: "IX_Groups_Group");

            migrationBuilder.AddColumn<long>(
                name: "LeaderId",
                table: "Groups",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LeaderId",
                table: "Groups",
                column: "LeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GuildMembers_LeaderId",
                table: "Groups",
                column: "LeaderId",
                principalTable: "GuildMembers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Groups_GroupEntityId",
                table: "GuildMembers",
                column: "GroupEntityId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Members_MemberEntityId",
                table: "GuildMembers",
                column: "MemberEntityId",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
