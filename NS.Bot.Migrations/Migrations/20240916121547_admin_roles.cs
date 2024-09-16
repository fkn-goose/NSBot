using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class admin_roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GuildMembers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "GuildMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GuildRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RelatedGuildId = table.Column<long>(type: "bigint", nullable: true),
                    PlayerRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    HelperRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    JuniorCuratorRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CuratorRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SeniorCuratorRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RPAdminRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChiefAdminDeputyRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChiefAdminRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    JuniorEventmasterRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    EventmasterRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChiefEventmasterRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    AdminListMessageChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    AdminListMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildRoles_Guilds_RelatedGuildId",
                        column: x => x.RelatedGuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildRoles_RelatedGuildId",
                table: "GuildRoles",
                column: "RelatedGuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildMembers_Groups_Id",
                table: "GuildMembers",
                column: "Id",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildMembers_Groups_Id",
                table: "GuildMembers");

            migrationBuilder.DropTable(
                name: "GuildRoles");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "GuildMembers");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GuildMembers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
