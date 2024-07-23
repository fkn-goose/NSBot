using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ticketentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<long>(type: "bigint", nullable: true),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    DeleteTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false)
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
                    GuildId = table.Column<long>(type: "bigint", nullable: true),
                    NewHelperTicketsCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewHelperTicketsChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OldHelperTicketsCategory = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewCuratorTicketsCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewCuratorTicketsChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OldCuratorTicketsCategory = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewAdminTicketsCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    NewAdminTicketsChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OldAdminTicketsCategory = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TicketLogs = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    HelperTicketsCount = table.Column<long>(type: "bigint", nullable: false),
                    CuratorTicketsCount = table.Column<long>(type: "bigint", nullable: false),
                    AdminTicketsCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketSettings_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_GuildId",
                table: "Tickets",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSettings_GuildId",
                table: "TicketSettings",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TicketSettings");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
