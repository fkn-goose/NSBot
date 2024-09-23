using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class switch_warn_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Radios_Guilds_GuildId",
                table: "Radios");

            migrationBuilder.DropForeignKey(
                name: "FK_Warns_Members_IssuedToId",
                table: "Warns");

            migrationBuilder.DropForeignKey(
                name: "FK_Warns_Members_ResponsibleId",
                table: "Warns");

            migrationBuilder.DropColumn(
                name: "IsReadOnly",
                table: "Warns");

            migrationBuilder.DropColumn(
                name: "IsRebuke",
                table: "Warns");

            migrationBuilder.DropColumn(
                name: "IsVerbal",
                table: "Warns");

            migrationBuilder.AddColumn<decimal>(
                name: "FirstRebukeRoleId",
                table: "WarnSettings",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondRebukeRoleId",
                table: "WarnSettings",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThirdRebukeRoleId",
                table: "WarnSettings",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<long>(
                name: "ResponsibleId",
                table: "Warns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "IssuedToId",
                table: "Warns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarnType",
                table: "Warns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Radios",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Radios_Guilds_GuildId",
                table: "Radios",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Warns_Members_IssuedToId",
                table: "Warns",
                column: "IssuedToId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Warns_Members_ResponsibleId",
                table: "Warns",
                column: "ResponsibleId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Radios_Guilds_GuildId",
                table: "Radios");

            migrationBuilder.DropForeignKey(
                name: "FK_Warns_Members_IssuedToId",
                table: "Warns");

            migrationBuilder.DropForeignKey(
                name: "FK_Warns_Members_ResponsibleId",
                table: "Warns");

            migrationBuilder.DropColumn(
                name: "FirstRebukeRoleId",
                table: "WarnSettings");

            migrationBuilder.DropColumn(
                name: "SecondRebukeRoleId",
                table: "WarnSettings");

            migrationBuilder.DropColumn(
                name: "ThirdRebukeRoleId",
                table: "WarnSettings");

            migrationBuilder.DropColumn(
                name: "WarnType",
                table: "Warns");

            migrationBuilder.AlterColumn<long>(
                name: "ResponsibleId",
                table: "Warns",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedToId",
                table: "Warns",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "IsReadOnly",
                table: "Warns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRebuke",
                table: "Warns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerbal",
                table: "Warns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Radios",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Radios_Guilds_GuildId",
                table: "Radios",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Warns_Members_IssuedToId",
                table: "Warns",
                column: "IssuedToId",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Warns_Members_ResponsibleId",
                table: "Warns",
                column: "ResponsibleId",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
