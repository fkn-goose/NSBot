using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NS.Bot.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class warn_RO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Permanent",
                table: "Warns",
                newName: "IsReadOnly");

            migrationBuilder.AddColumn<bool>(
                name: "IsPermanent",
                table: "Warns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MessageId",
                table: "Warns",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPermanent",
                table: "Warns");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Warns");

            migrationBuilder.RenameColumn(
                name: "IsReadOnly",
                table: "Warns",
                newName: "Permanent");
        }
    }
}
