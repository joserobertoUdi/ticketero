using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivoFijoAuditoriaAndRemoveKioskoMediaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KioskoMediaId",
                schema: "dbo",
                table: "Kiosko");

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                schema: "dbo",
                table: "ActivosFijos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                schema: "dbo",
                table: "ActivosFijos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UltimaModificacionPor",
                schema: "dbo",
                table: "ActivosFijos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivosFijosAuditoria",
                schema: "dbo",
                columns: table => new
                {
                    AuditoriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivoFijoId = table.Column<int>(type: "int", nullable: true),
                    KioskoId = table.Column<int>(type: "int", nullable: true),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CambioResumen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosFijosAuditoria", x => x.AuditoriaId);
                    table.ForeignKey(
                        name: "FK_ActivosFijosAuditoria_ActivosFijos_ActivoFijoId",
                        column: x => x.ActivoFijoId,
                        principalSchema: "dbo",
                        principalTable: "ActivosFijos",
                        principalColumn: "ActivoFijoId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActivosFijosAuditoria_Kiosko_KioskoId",
                        column: x => x.KioskoId,
                        principalSchema: "dbo",
                        principalTable: "Kiosko",
                        principalColumn: "KioskoId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijosAuditoria_ActivoFijoId",
                schema: "dbo",
                table: "ActivosFijosAuditoria",
                column: "ActivoFijoId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijosAuditoria_Fecha",
                schema: "dbo",
                table: "ActivosFijosAuditoria",
                column: "FechaCambio");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijosAuditoria_KioskoId",
                schema: "dbo",
                table: "ActivosFijosAuditoria",
                column: "KioskoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivosFijosAuditoria",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "CreadoPor",
                schema: "dbo",
                table: "ActivosFijos");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                schema: "dbo",
                table: "ActivosFijos");

            migrationBuilder.DropColumn(
                name: "UltimaModificacionPor",
                schema: "dbo",
                table: "ActivosFijos");

            migrationBuilder.AddColumn<int>(
                name: "KioskoMediaId",
                schema: "dbo",
                table: "Kiosko",
                type: "int",
                nullable: true);
        }
    }
}
