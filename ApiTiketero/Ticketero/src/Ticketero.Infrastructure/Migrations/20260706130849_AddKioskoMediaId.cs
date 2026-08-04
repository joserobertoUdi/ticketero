using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKioskoMediaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KioskoMediaId",
                schema: "dbo",
                table: "Kiosko",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KioskoMediaId",
                schema: "dbo",
                table: "Kiosko");
        }
    }
}
