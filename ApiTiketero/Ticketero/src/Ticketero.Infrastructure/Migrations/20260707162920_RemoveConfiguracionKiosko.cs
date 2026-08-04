using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConfiguracionKiosko : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionKiosko",
                schema: "dbo");

            migrationBuilder.AddColumn<string>(
                name: "DnsEquipo",
                schema: "dbo",
                table: "Kiosko",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayEquipo",
                schema: "dbo",
                table: "Kiosko",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpEquipo",
                schema: "dbo",
                table: "Kiosko",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MascaraRedEquipo",
                schema: "dbo",
                table: "Kiosko",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DnsEquipo",
                schema: "dbo",
                table: "Kiosko");

            migrationBuilder.DropColumn(
                name: "GatewayEquipo",
                schema: "dbo",
                table: "Kiosko");

            migrationBuilder.DropColumn(
                name: "IpEquipo",
                schema: "dbo",
                table: "Kiosko");

            migrationBuilder.DropColumn(
                name: "MascaraRedEquipo",
                schema: "dbo",
                table: "Kiosko");

            migrationBuilder.CreateTable(
                name: "ConfiguracionKiosko",
                schema: "dbo",
                columns: table => new
                {
                    ConfiguracionKioskoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KioskoId = table.Column<int>(type: "int", nullable: false),
                    DNS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    EstadoImpresora = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EstadoMultimedia = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormatoMultimedia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Gateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Impresora = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpEquipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpImpresora = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MascaraRed = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PuertoImpresora = table.Column<int>(type: "int", nullable: true),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RutaMultimedia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TiempoMultimediaSegundos = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionKiosko", x => x.ConfiguracionKioskoId);
                    table.ForeignKey(
                        name: "FK_ConfiguracionKiosko_Kiosko_KioskoId",
                        column: x => x.KioskoId,
                        principalSchema: "dbo",
                        principalTable: "Kiosko",
                        principalColumn: "KioskoId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionKiosko_Estado",
                schema: "dbo",
                table: "ConfiguracionKiosko",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "UQ_ConfiguracionKiosko",
                schema: "dbo",
                table: "ConfiguracionKiosko",
                column: "KioskoId",
                unique: true);
        }
    }
}
