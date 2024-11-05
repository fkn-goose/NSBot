using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class nohelpersettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildData");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TicketSettings");

            migrationBuilder.CreateTable(
                name: "BaseTickets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: false),
                    ResponsibleId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    DeleteTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    NewNick = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseTickets_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseTickets_GuildId",
                table: "BaseTickets",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseTickets");

            migrationBuilder.CreateTable(
                name: "GuildData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RelatedGuildId = table.Column<long>(type: "bigint", nullable: false),
                    JDHVoiceId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    JDKVoiceId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ZoneVoiceId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildData_Guilds_RelatedGuildId",
                        column: x => x.RelatedGuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<long>(type: "bigint", nullable: true),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DeleteTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TicketSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RelatedId = table.Column<long>(type: "bigint", nullable: true),
                    AdminTicketsCount = table.Column<long>(type: "bigint", nullable: false),
                    CuratorTicketsCount = table.Column<long>(type: "bigint", nullable: false),
                    HelperTicketsCount = table.Column<long>(type: "bigint", nullable: false),
                    NewAdminTicketsCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewAdminTicketsChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewCuratorTicketsCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewCuratorTicketsChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewHelperTicketsCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewHelperTicketsChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OldAdminTicketsCategory = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OldCuratorTicketsCategory = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OldHelperTicketsCategory = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TicketLogs = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketSettings_Guilds_RelatedId",
                        column: x => x.RelatedId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildData_RelatedGuildId",
                table: "GuildData",
                column: "RelatedGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_GuildId",
                table: "Tickets",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSettings_RelatedId",
                table: "TicketSettings",
                column: "RelatedId");
        }
    }
}
