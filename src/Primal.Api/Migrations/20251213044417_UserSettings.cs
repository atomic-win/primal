using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Primal.Api.Migrations
{
    /// <inheritdoc />
    public partial class UserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredCurrency",
                table: "users",
                type: "TEXT",
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "PreferredLocale",
                table: "users",
                type: "TEXT",
                nullable: false,
                defaultValue: "EN_US");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredCurrency",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PreferredLocale",
                table: "users");
        }
    }
}
