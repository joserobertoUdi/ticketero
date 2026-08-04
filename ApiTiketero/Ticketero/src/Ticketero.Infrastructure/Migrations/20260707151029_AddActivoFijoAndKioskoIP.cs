using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivoFijoAndKioskoIP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpKiosko",
                schema: "dbo",
                table: "Kiosko",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivosFijos",
                schema: "dbo",
                columns: table => new
                {
                    ActivoFijoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KioskoId = table.Column<int>(type: "int", nullable: false),
                    TipoActivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroActivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Serie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosFijos", x => x.ActivoFijoId);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_Kiosko_KioskoId",
                        column: x => x.KioskoId,
                        principalSchema: "dbo",
                        principalTable: "Kiosko",
                        principalColumn: "KioskoId");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ActivosFijos",
                schema: "dbo",
                table: "ActivosFijos",
                columns: new[] { "KioskoId", "NumeroActivo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivosFijos",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "IpKiosko",
                schema: "dbo",
                table: "Kiosko");
        }
    }
}
