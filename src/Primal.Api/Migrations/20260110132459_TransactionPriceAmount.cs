using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Primal.Api.Migrations
{
    /// <inheritdoc />
    public partial class TransactionPriceAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "transactions");
        }
    }
}
