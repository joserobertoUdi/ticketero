using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAreaIcono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marcacion_Kiosko_KioskoId",
                schema: "dbo",
                table: "Marcacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Marcacion_Tickets_TicketId",
                schema: "dbo",
                table: "Marcacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Marcacion_Usuarios_UsuarioId",
                schema: "dbo",
                table: "Marcacion");

            migrationBuilder.DropIndex(
                name: "IX_Atencion_TicketId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.DropColumn(
                name: "Icono",
                schema: "dbo",
                table: "AreasFase");

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_TicketId_Activa",
                schema: "dbo",
                table: "Atencion",
                columns: new[] { "TicketId", "FechaFin" },
                unique: true,
                filter: "[FechaFin] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacion_Kiosko_KioskoId",
                schema: "dbo",
                table: "Marcacion",
                column: "KioskoId",
                principalSchema: "dbo",
                principalTable: "Kiosko",
                principalColumn: "KioskoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacion_Tickets_TicketId",
                schema: "dbo",
                table: "Marcacion",
                column: "TicketId",
                principalSchema: "dbo",
                principalTable: "Tickets",
                principalColumn: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacion_Usuarios_UsuarioId",
                schema: "dbo",
                table: "Marcacion",
                column: "UsuarioId",
                principalSchema: "dbo",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marcacion_Kiosko_KioskoId",
                schema: "dbo",
                table: "Marcacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Marcacion_Tickets_TicketId",
                schema: "dbo",
                table: "Marcacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Marcacion_Usuarios_UsuarioId",
                schema: "dbo",
                table: "Marcacion");

            migrationBuilder.DropIndex(
                name: "IX_Atencion_TicketId_Activa",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.AddColumn<byte[]>(
                name: "Icono",
                schema: "dbo",
                table: "AreasFase",
                type: "image",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_TicketId",
                schema: "dbo",
                table: "Atencion",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacion_Kiosko_KioskoId",
                schema: "dbo",
                table: "Marcacion",
                column: "KioskoId",
                principalSchema: "dbo",
                principalTable: "Kiosko",
                principalColumn: "KioskoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacion_Tickets_TicketId",
                schema: "dbo",
                table: "Marcacion",
                column: "TicketId",
                principalSchema: "dbo",
                principalTable: "Tickets",
                principalColumn: "TicketId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacion_Usuarios_UsuarioId",
                schema: "dbo",
                table: "Marcacion",
                column: "UsuarioId",
                principalSchema: "dbo",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
