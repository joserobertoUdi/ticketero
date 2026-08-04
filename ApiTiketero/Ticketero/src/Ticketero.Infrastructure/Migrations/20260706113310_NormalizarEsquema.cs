using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizarEsquema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UbicacionId",
                schema: "dbo",
                table: "Kiosko",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ConfiguracionImpresora",
                schema: "dbo",
                columns: table => new
                {
                    ConfiguracionImpresoraId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KioskoId = table.Column<int>(type: "int", nullable: false),
                    NombreImpresora = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Puerto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DireccionIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TipoConexion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AnchoPapelMM = table.Column<int>(type: "int", nullable: false),
                    Copias = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    ImpresionAutomatica = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionImpresora", x => x.ConfiguracionImpresoraId);
                    table.ForeignKey(
                        name: "FK_ConfiguracionImpresora_Kiosko_KioskoId",
                        column: x => x.KioskoId,
                        principalSchema: "dbo",
                        principalTable: "Kiosko",
                        principalColumn: "KioskoId");
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionMultimedia",
                schema: "dbo",
                columns: table => new
                {
                    ConfiguracionMultimediaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KioskoId = table.Column<int>(type: "int", nullable: false),
                    NombreContenido = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DuracionSegundos = table.Column<int>(type: "int", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Repetir = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionMultimedia", x => x.ConfiguracionMultimediaId);
                    table.ForeignKey(
                        name: "FK_ConfiguracionMultimedia_Kiosko_KioskoId",
                        column: x => x.KioskoId,
                        principalSchema: "dbo",
                        principalTable: "Kiosko",
                        principalColumn: "KioskoId");
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionRed",
                schema: "dbo",
                columns: table => new
                {
                    ConfiguracionRedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KioskoId = table.Column<int>(type: "int", nullable: false),
                    TipoConexion = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DHCP = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DireccionIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MascaraSubred = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DNSPrimario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DNSSecundario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Puerto = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionRed", x => x.ConfiguracionRedId);
                    table.ForeignKey(
                        name: "FK_ConfiguracionRed_Kiosko_KioskoId",
                        column: x => x.KioskoId,
                        principalSchema: "dbo",
                        principalTable: "Kiosko",
                        principalColumn: "KioskoId");
                });

            migrationBuilder.CreateTable(
                name: "KioskoAreas",
                schema: "dbo",
                columns: table => new
                {
                    KioskoAreaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KioskoId = table.Column<int>(type: "int", nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskoAreas", x => x.KioskoAreaId);
                    table.ForeignKey(
                        name: "FK_KioskoAreas_AreasFase_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "dbo",
                        principalTable: "AreasFase",
                        principalColumn: "AreaId");
                    table.ForeignKey(
                        name: "FK_KioskoAreas_Kiosko_KioskoId",
                        column: x => x.KioskoId,
                        principalSchema: "dbo",
                        principalTable: "Kiosko",
                        principalColumn: "KioskoId");
                });

            migrationBuilder.CreateTable(
                name: "Ubicaciones",
                schema: "dbo",
                columns: table => new
                {
                    UbicacionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Edificio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Piso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ubicaciones", x => x.UbicacionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kiosko_UbicacionId",
                schema: "dbo",
                table: "Kiosko",
                column: "UbicacionId");

            migrationBuilder.CreateIndex(
                name: "UQ_ConfiguracionImpresora",
                schema: "dbo",
                table: "ConfiguracionImpresora",
                columns: new[] { "KioskoId", "NombreImpresora" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ConfiguracionMultimedia",
                schema: "dbo",
                table: "ConfiguracionMultimedia",
                columns: new[] { "KioskoId", "NombreContenido" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ConfiguracionRed",
                schema: "dbo",
                table: "ConfiguracionRed",
                columns: new[] { "KioskoId", "DireccionIP" },
                unique: true,
                filter: "[DireccionIP] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KioskoAreas_AreaId",
                schema: "dbo",
                table: "KioskoAreas",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "UQ_KioskoAreas",
                schema: "dbo",
                table: "KioskoAreas",
                columns: new[] { "KioskoId", "AreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Ubicaciones",
                schema: "dbo",
                table: "Ubicaciones",
                columns: new[] { "Edificio", "Piso", "Sector", "Descripcion" },
                unique: true);

            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.Ubicaciones ON; INSERT INTO dbo.Ubicaciones (UbicacionId, Descripcion, Edificio, Piso, Sector, Estado, FechaReg, Ride) VALUES (1, 'Default', 'Default', 'Default', 'Default', 1, SYSDATETIME(), NEWID()); SET IDENTITY_INSERT dbo.Ubicaciones OFF;");

            migrationBuilder.AddForeignKey(
                name: "FK_Kiosko_Ubicaciones_UbicacionId",
                schema: "dbo",
                table: "Kiosko",
                column: "UbicacionId",
                principalSchema: "dbo",
                principalTable: "Ubicaciones",
                principalColumn: "UbicacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kiosko_Ubicaciones_UbicacionId",
                schema: "dbo",
                table: "Kiosko");

            migrationBuilder.DropTable(
                name: "ConfiguracionImpresora",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ConfiguracionMultimedia",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ConfiguracionRed",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KioskoAreas",
                schema: "dbo");

            migrationBuilder.Sql("DELETE FROM dbo.Ubicaciones WHERE UbicacionId = 1;");

            migrationBuilder.DropTable(
                name: "Ubicaciones",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Kiosko_UbicacionId",
                schema: "dbo",
                table: "Kiosko");

            migrationBuilder.DropColumn(
                name: "UbicacionId",
                schema: "dbo",
                table: "Kiosko");
        }
    }
}
