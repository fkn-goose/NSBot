using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class member_fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Groups_GroupEntityId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Guilds_GuildId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GroupMembers_LeaderId",
                table: "Groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupMembers",
                table: "GroupMembers");

            migrationBuilder.RenameTable(
                name: "GroupMembers",
                newName: "GuildMembers");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMembers_GuildId",
                table: "GuildMembers",
                newName: "IX_GuildMembers_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMembers_GroupEntityId",
                table: "GuildMembers",
                newName: "IX_GuildMembers_GroupEntityId");

            migrationBuilder.AddColumn<long>(
                name: "MemberEntityId",
                table: "GuildMembers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildMembers",
                table: "GuildMembers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SteamId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildMembers_MemberEntityId",
                table: "GuildMembers",
                column: "MemberEntityId");

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
                name: "FK_GuildMembers_Guilds_GuildId",
                table: "GuildMembers",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Members_MemberEntityId",
                table: "GuildMembers",
                column: "MemberEntityId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GuildMembers_LeaderId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Groups_GroupEntityId",
                table: "GuildMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Guilds_GuildId",
                table: "GuildMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Members_MemberEntityId",
                table: "GuildMembers");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildMembers",
                table: "GuildMembers");

            migrationBuilder.DropIndex(
                name: "IX_GuildMembers_MemberEntityId",
                table: "GuildMembers");

            migrationBuilder.DropColumn(
                name: "MemberEntityId",
                table: "GuildMembers");

            migrationBuilder.RenameTable(
                name: "GuildMembers",
                newName: "GroupMembers");

            migrationBuilder.RenameIndex(
                name: "IX_GuildMembers_GuildId",
                table: "GroupMembers",
                newName: "IX_GroupMembers_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildMembers_GroupEntityId",
                table: "GroupMembers",
                newName: "IX_GroupMembers_GroupEntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupMembers",
                table: "GroupMembers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Groups_GroupEntityId",
                table: "GroupMembers",
                column: "GroupEntityId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Guilds_GuildId",
                table: "GroupMembers",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GroupMembers_LeaderId",
                table: "Groups",
                column: "LeaderId",
                principalTable: "GroupMembers",
                principalColumn: "Id");
        }
    }
}
