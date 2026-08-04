using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAtencionCascadeAndAddNombreUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atencion_EstadosTicket_EstadoTicketId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.DropForeignKey(
                name: "FK_Atencion_Servicios_ServicioId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.DropForeignKey(
                name: "FK_Atencion_Tickets_TicketId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.DropForeignKey(
                name: "FK_Atencion_Usuarios_UsuarioId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.AddColumn<string>(
                name: "NombreUsuario",
                schema: "dbo",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Atencion_EstadosTicket_EstadoTicketId",
                schema: "dbo",
                table: "Atencion",
                column: "EstadoTicketId",
                principalSchema: "dbo",
                principalTable: "EstadosTicket",
                principalColumn: "EstadoTicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Atencion_Servicios_ServicioId",
                schema: "dbo",
                table: "Atencion",
                column: "ServicioId",
                principalSchema: "dbo",
                principalTable: "Servicios",
                principalColumn: "ServicioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Atencion_Tickets_TicketId",
                schema: "dbo",
                table: "Atencion",
                column: "TicketId",
                principalSchema: "dbo",
                principalTable: "Tickets",
                principalColumn: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Atencion_Usuarios_UsuarioId",
                schema: "dbo",
                table: "Atencion",
                column: "UsuarioId",
                principalSchema: "dbo",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atencion_EstadosTicket_EstadoTicketId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.DropForeignKey(
                name: "FK_Atencion_Servicios_ServicioId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.DropForeignKey(
                name: "FK_Atencion_Tickets_TicketId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.DropForeignKey(
                name: "FK_Atencion_Usuarios_UsuarioId",
                schema: "dbo",
                table: "Atencion");

            migrationBuilder.DropColumn(
                name: "NombreUsuario",
                schema: "dbo",
                table: "Usuarios");

            migrationBuilder.AddForeignKey(
                name: "FK_Atencion_EstadosTicket_EstadoTicketId",
                schema: "dbo",
                table: "Atencion",
                column: "EstadoTicketId",
                principalSchema: "dbo",
                principalTable: "EstadosTicket",
                principalColumn: "EstadoTicketId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Atencion_Servicios_ServicioId",
                schema: "dbo",
                table: "Atencion",
                column: "ServicioId",
                principalSchema: "dbo",
                principalTable: "Servicios",
                principalColumn: "ServicioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Atencion_Tickets_TicketId",
                schema: "dbo",
                table: "Atencion",
                column: "TicketId",
                principalSchema: "dbo",
                principalTable: "Tickets",
                principalColumn: "TicketId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Atencion_Usuarios_UsuarioId",
                schema: "dbo",
                table: "Atencion",
                column: "UsuarioId",
                principalSchema: "dbo",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
