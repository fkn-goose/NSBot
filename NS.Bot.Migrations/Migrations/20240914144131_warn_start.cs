using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class warn_start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RadioSettings_Guilds_GuildId",
                table: "RadioSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketSettings_Guilds_GuildId",
                table: "TicketSettings");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "TicketSettings",
                newName: "RelatedId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketSettings_GuildId",
                table: "TicketSettings",
                newName: "IX_TicketSettings_RelatedId");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "RadioSettings",
                newName: "RelatedGuildId");

            migrationBuilder.RenameIndex(
                name: "IX_RadioSettings_GuildId",
                table: "RadioSettings",
                newName: "IX_RadioSettings_RelatedGuildId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Groups",
                newName: "GroupType");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                newName: "IX_Groups_GroupType");

            migrationBuilder.AddColumn<long>(
                name: "TotalWarnCount",
                table: "Members",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Warns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResponsibleId = table.Column<long>(type: "bigint", nullable: true),
                    IssuedToId = table.Column<long>(type: "bigint", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<long>(type: "bigint", nullable: false),
                    Indefinite = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerbal = table.Column<bool>(type: "boolean", nullable: false),
                    IsRebuke = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warns_Members_IssuedToId",
                        column: x => x.IssuedToId,
                        principalTable: "Members",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Warns_Members_ResponsibleId",
                        column: x => x.ResponsibleId,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WarnSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RelatedGuildId = table.Column<long>(type: "bigint", nullable: true),
                    WarnChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    FirstWarnRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SecondWarnRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ThirdWarnRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BanRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ReadOnlyRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarnSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarnSettings_Guilds_RelatedGuildId",
                        column: x => x.RelatedGuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warns_IssuedToId",
                table: "Warns",
                column: "IssuedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Warns_ResponsibleId",
                table: "Warns",
                column: "ResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_WarnSettings_RelatedGuildId",
                table: "WarnSettings",
                column: "RelatedGuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_RadioSettings_Guilds_RelatedGuildId",
                table: "RadioSettings",
                column: "RelatedGuildId",
                principalTable: "Guilds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketSettings_Guilds_RelatedId",
                table: "TicketSettings",
                column: "RelatedId",
                principalTable: "Guilds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RadioSettings_Guilds_RelatedGuildId",
                table: "RadioSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketSettings_Guilds_RelatedId",
                table: "TicketSettings");

            migrationBuilder.DropTable(
                name: "Warns");

            migrationBuilder.DropTable(
                name: "WarnSettings");

            migrationBuilder.DropColumn(
                name: "TotalWarnCount",
                table: "Members");

            migrationBuilder.RenameColumn(
                name: "RelatedId",
                table: "TicketSettings",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketSettings_RelatedId",
                table: "TicketSettings",
                newName: "IX_TicketSettings_GuildId");

            migrationBuilder.RenameColumn(
                name: "RelatedGuildId",
                table: "RadioSettings",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_RadioSettings_RelatedGuildId",
                table: "RadioSettings",
                newName: "IX_RadioSettings_GuildId");

            migrationBuilder.RenameColumn(
                name: "GroupType",
                table: "Groups",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_GroupType",
                table: "Groups",
                newName: "IX_Groups_Name");

            migrationBuilder.AddForeignKey(
                name: "FK_RadioSettings_Guilds_GuildId",
                table: "RadioSettings",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketSettings_Guilds_GuildId",
                table: "TicketSettings",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");
        }
    }
}
