using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AreasFase",
                schema: "dbo",
                columns: table => new
                {
                    AreaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prefijo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icono = table.Column<byte[]>(type: "image", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreasFase", x => x.AreaId);
                });

            migrationBuilder.CreateTable(
                name: "Configuraciones",
                schema: "dbo",
                columns: table => new
                {
                    ConfiguracionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuraciones", x => x.ConfiguracionId);
                });

            migrationBuilder.CreateTable(
                name: "EstadosTicket",
                schema: "dbo",
                columns: table => new
                {
                    EstadoTicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosTicket", x => x.EstadoTicketId);
                });

            migrationBuilder.CreateTable(
                name: "Kiosko",
                schema: "dbo",
                columns: table => new
                {
                    KioskoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kiosko", x => x.KioskoId);
                });

            migrationBuilder.CreateTable(
                name: "Prioridades",
                schema: "dbo",
                columns: table => new
                {
                    PrioridadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nivel = table.Column<byte>(type: "tinyint", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prioridades", x => x.PrioridadId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "dbo",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Logs = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RolId);
                });

            migrationBuilder.CreateTable(
                name: "TiposTicket",
                schema: "dbo",
                columns: table => new
                {
                    TipoTicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposTicket", x => x.TipoTicketId);
                });

            migrationBuilder.CreateTable(
                name: "Puestos",
                schema: "dbo",
                columns: table => new
                {
                    PuestoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puestos", x => x.PuestoId);
                    table.ForeignKey(
                        name: "FK_Puestos_AreasFase_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "dbo",
                        principalTable: "AreasFase",
                        principalColumn: "AreaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                schema: "dbo",
                columns: table => new
                {
                    ServicioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.ServicioId);
                    table.ForeignKey(
                        name: "FK_Servicios_AreasFase_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "dbo",
                        principalTable: "AreasFase",
                        principalColumn: "AreaId");
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionKiosko",
                schema: "dbo",
                columns: table => new
                {
                    ConfiguracionKioskoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KioskoId = table.Column<int>(type: "int", nullable: false),
                    Impresora = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpImpresora = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PuertoImpresora = table.Column<int>(type: "int", nullable: true),
                    EstadoImpresora = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RutaMultimedia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FormatoMultimedia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TiempoMultimediaSegundos = table.Column<int>(type: "int", nullable: true),
                    EstadoMultimedia = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IpEquipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MascaraRed = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DNS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "dbo",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodigoSistema = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    Logs = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaUltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "RolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                schema: "dbo",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroTicket = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    TipoTicketId = table.Column<int>(type: "int", nullable: false),
                    PrioridadId = table.Column<int>(type: "int", nullable: false),
                    EstadoTicketId = table.Column<int>(type: "int", nullable: false),
                    AreaActualId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_Tickets_AreasFase_AreaActualId",
                        column: x => x.AreaActualId,
                        principalSchema: "dbo",
                        principalTable: "AreasFase",
                        principalColumn: "AreaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_EstadosTicket_EstadoTicketId",
                        column: x => x.EstadoTicketId,
                        principalSchema: "dbo",
                        principalTable: "EstadosTicket",
                        principalColumn: "EstadoTicketId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Prioridades_PrioridadId",
                        column: x => x.PrioridadId,
                        principalSchema: "dbo",
                        principalTable: "Prioridades",
                        principalColumn: "PrioridadId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalSchema: "dbo",
                        principalTable: "Servicios",
                        principalColumn: "ServicioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_TiposTicket_TipoTicketId",
                        column: x => x.TipoTicketId,
                        principalSchema: "dbo",
                        principalTable: "TiposTicket",
                        principalColumn: "TipoTicketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SesionOperador",
                schema: "dbo",
                columns: table => new
                {
                    SesionOperadorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PuestoId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionOperador", x => x.SesionOperadorId);
                    table.ForeignKey(
                        name: "FK_SesionOperador_Puestos_PuestoId",
                        column: x => x.PuestoId,
                        principalSchema: "dbo",
                        principalTable: "Puestos",
                        principalColumn: "PuestoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SesionOperador_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosAreaFase",
                schema: "dbo",
                columns: table => new
                {
                    UsuarioAreaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosAreaFase", x => x.UsuarioAreaId);
                    table.ForeignKey(
                        name: "FK_UsuariosAreaFase_AreasFase_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "dbo",
                        principalTable: "AreasFase",
                        principalColumn: "AreaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosAreaFase_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Marcacion",
                schema: "dbo",
                columns: table => new
                {
                    MarcacionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    KioskoId = table.Column<int>(type: "int", nullable: false),
                    NumeroLlamado = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaMarcacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Respondio = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcacion", x => x.MarcacionId);
                    table.ForeignKey(
                        name: "FK_Marcacion_Kiosko_KioskoId",
                        column: x => x.KioskoId,
                        principalSchema: "dbo",
                        principalTable: "Kiosko",
                        principalColumn: "KioskoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Marcacion_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "dbo",
                        principalTable: "Tickets",
                        principalColumn: "TicketId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Marcacion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Atencion",
                schema: "dbo",
                columns: table => new
                {
                    AtencionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    EstadoTicketId = table.Column<int>(type: "int", nullable: false),
                    SesionOperadorId = table.Column<int>(type: "int", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TiempoAtencionSegundos = table.Column<int>(type: "int", nullable: true),
                    FueDerivado = table.Column<bool>(type: "bit", nullable: false),
                    AreaDestinoId = table.Column<int>(type: "int", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaReg = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ride = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atencion", x => x.AtencionId);
                    table.ForeignKey(
                        name: "FK_Atencion_AreasFase_AreaDestinoId",
                        column: x => x.AreaDestinoId,
                        principalSchema: "dbo",
                        principalTable: "AreasFase",
                        principalColumn: "AreaId");
                    table.ForeignKey(
                        name: "FK_Atencion_AreasFase_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "dbo",
                        principalTable: "AreasFase",
                        principalColumn: "AreaId");
                    table.ForeignKey(
                        name: "FK_Atencion_EstadosTicket_EstadoTicketId",
                        column: x => x.EstadoTicketId,
                        principalSchema: "dbo",
                        principalTable: "EstadosTicket",
                        principalColumn: "EstadoTicketId");
                    table.ForeignKey(
                        name: "FK_Atencion_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalSchema: "dbo",
                        principalTable: "Servicios",
                        principalColumn: "ServicioId");
                    table.ForeignKey(
                        name: "FK_Atencion_SesionOperador_SesionOperadorId",
                        column: x => x.SesionOperadorId,
                        principalSchema: "dbo",
                        principalTable: "SesionOperador",
                        principalColumn: "SesionOperadorId");
                    table.ForeignKey(
                        name: "FK_Atencion_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "dbo",
                        principalTable: "Tickets",
                        principalColumn: "TicketId");
                    table.ForeignKey(
                        name: "FK_Atencion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_AreasFase_Descripcion",
                schema: "dbo",
                table: "AreasFase",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_AreasFase_Prefijo",
                schema: "dbo",
                table: "AreasFase",
                column: "Prefijo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_AreaDestinoId",
                schema: "dbo",
                table: "Atencion",
                column: "AreaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_AreaId",
                schema: "dbo",
                table: "Atencion",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_EstadoTicketId",
                schema: "dbo",
                table: "Atencion",
                column: "EstadoTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_ServicioId",
                schema: "dbo",
                table: "Atencion",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_SesionOperadorId",
                schema: "dbo",
                table: "Atencion",
                column: "SesionOperadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_TicketId",
                schema: "dbo",
                table: "Atencion",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_UsuarioId",
                schema: "dbo",
                table: "Atencion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Configuraciones_Estado",
                schema: "dbo",
                table: "Configuraciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "UQ_Configuraciones_Clave",
                schema: "dbo",
                table: "Configuraciones",
                column: "Clave",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "UQ_EstadosTicket_Descripcion",
                schema: "dbo",
                table: "EstadosTicket",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Kiosko_Descripcion",
                schema: "dbo",
                table: "Kiosko",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marcacion_KioskoId",
                schema: "dbo",
                table: "Marcacion",
                column: "KioskoId");

            migrationBuilder.CreateIndex(
                name: "IX_Marcacion_TicketId",
                schema: "dbo",
                table: "Marcacion",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Marcacion_UsuarioId",
                schema: "dbo",
                table: "Marcacion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UQ_Prioridades_Descripcion",
                schema: "dbo",
                table: "Prioridades",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Prioridades_Nivel",
                schema: "dbo",
                table: "Prioridades",
                column: "Nivel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Puestos_Area_Descripcion",
                schema: "dbo",
                table: "Puestos",
                columns: new[] { "AreaId", "Descripcion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Roles_Descripcion",
                schema: "dbo",
                table: "Roles",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Servicios_Area_Descripcion",
                schema: "dbo",
                table: "Servicios",
                columns: new[] { "AreaId", "Descripcion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SesionOperador_PuestoId",
                schema: "dbo",
                table: "SesionOperador",
                column: "PuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionOperador_UsuarioId",
                schema: "dbo",
                table: "SesionOperador",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AreaActualId",
                schema: "dbo",
                table: "Tickets",
                column: "AreaActualId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EstadoTicketId",
                schema: "dbo",
                table: "Tickets",
                column: "EstadoTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PrioridadId",
                schema: "dbo",
                table: "Tickets",
                column: "PrioridadId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ServicioId",
                schema: "dbo",
                table: "Tickets",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TipoTicketId",
                schema: "dbo",
                table: "Tickets",
                column: "TipoTicketId");

            migrationBuilder.CreateIndex(
                name: "UQ_Tickets_NumeroTicket",
                schema: "dbo",
                table: "Tickets",
                column: "NumeroTicket",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TiposTicket_Descripcion",
                schema: "dbo",
                table: "TiposTicket",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                schema: "dbo",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_CodigoSistema",
                schema: "dbo",
                table: "Usuarios",
                column: "CodigoSistema",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_Correo",
                schema: "dbo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosAreaFase_AreaId",
                schema: "dbo",
                table: "UsuariosAreaFase",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "UQ_UsuariosAreaFase",
                schema: "dbo",
                table: "UsuariosAreaFase",
                columns: new[] { "UsuarioId", "AreaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atencion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Configuraciones",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ConfiguracionKiosko",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Marcacion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UsuariosAreaFase",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SesionOperador",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Kiosko",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Puestos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EstadosTicket",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Prioridades",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Servicios",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TiposTicket",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AreasFase",
                schema: "dbo");
        }
    }
}
