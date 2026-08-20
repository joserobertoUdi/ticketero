using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Ticketero.Application.DTOs;
using Ticketero.Application.UseCases;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;
using Ticketero.Infrastructure.Data;
using Ticketero.Infrastructure.Repositories;

namespace Ticketero.Tests;

public class TicketFlujoIntegracionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TicketeroDbContext _context;

    public TicketFlujoIntegracionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TicketeroDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TicketeroDbContext(options);
        _context.Database.EnsureCreated();
    }

    private async Task<Area> SeedAreaAsync(string prefijo = "CAJ", string descripcion = "Caja")
    {
        var area = new Area { Descripcion = descripcion, Prefijo = prefijo };
        _context.Areas.Add(area);
        await _context.SaveChangesAsync();
        return area;
    }

    private async Task<Prioridad> SeedPrioridadAsync(byte nivel = 1)
    {
        var prioridad = new Prioridad { Descripcion = $"P{nivel}", Nivel = nivel };
        _context.Prioridades.Add(prioridad);
        await _context.SaveChangesAsync();
        return prioridad;
    }

    private async Task<(Servicio Servicio, TipoTicket TipoTicket)> SeedReferenciasAsync()
    {
        var servicio = new Servicio { Descripcion = "General", AreaId = 1 };
        var tipoTicket = new TipoTicket { Descripcion = "Normal" };
        _context.Servicios.Add(servicio);
        _context.TiposTicket.Add(tipoTicket);

        var estados = new (int Id, string Descripcion)[]
        {
            (TicketEstado.Nuevo, "Nuevo"),
            (TicketEstado.Asignado, "Asignado"),
            (TicketEstado.EnAtencion, "En Atendimiento"),
            (TicketEstado.EnEspera, "En Espera"),
            (TicketEstado.Resuelto, "Resuelto"),
            (TicketEstado.Cerrado, "Cerrado"),
            (TicketEstado.Cancelado, "Cancelado"),
            (TicketEstado.Llamado, "Llamado")
        };
        foreach (var (id, descripcion) in estados)
        {
            _context.EstadosTicket.Add(new EstadoTicket { Descripcion = descripcion });
        }

        await _context.SaveChangesAsync();
        return (servicio, tipoTicket);
    }

    private async Task<Ticket> SeedTicketAsync(int estado, Area area, Prioridad prioridad, string numero, Servicio? servicio = null, TipoTicket? tipoTicket = null)
    {
        var ticket = new Ticket
        {
            NumeroTicket = numero,
            ServicioId = servicio?.Id ?? 1,
            TipoTicketId = tipoTicket?.Id ?? 1,
            PrioridadId = prioridad.Id,
            EstadoTicketId = estado,
            AreaActualId = area.Id,
            FechaCreacion = DateTime.UtcNow
        };
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    [Fact]
    public async Task ColaPendientes_SoloDevuelveTicketsNuevoYEnEspera()
    {
        // Caso 2: la cola por llamar excluye tickets Asignado/EnAtencion/
        // Cerrado/Cancelado/Llamado.
        var area = await SeedAreaAsync();
        var prioridad = await SeedPrioridadAsync();
        var refs = await SeedReferenciasAsync();
        var tNuevo = await SeedTicketAsync(TicketEstado.Nuevo, area, prioridad, "CAJ-001", refs.Servicio, refs.TipoTicket);
        var tEspera = await SeedTicketAsync(TicketEstado.EnEspera, area, prioridad, "CAJ-002", refs.Servicio, refs.TipoTicket);
        var tAsignado = await SeedTicketAsync(TicketEstado.Asignado, area, prioridad, "CAJ-003", refs.Servicio, refs.TipoTicket);
        var tEnAtencion = await SeedTicketAsync(TicketEstado.EnAtencion, area, prioridad, "CAJ-004", refs.Servicio, refs.TipoTicket);
        var tCerrado = await SeedTicketAsync(TicketEstado.Cerrado, area, prioridad, "CAJ-005", refs.Servicio, refs.TipoTicket);
        var tLlamado = await SeedTicketAsync(TicketEstado.Llamado, area, prioridad, "CAJ-006", refs.Servicio, refs.TipoTicket);

        var uow = new UnitOfWork(_context);
        var pendientes = await uow.Tickets.GetTicketsPendientesPorAreaAsync(area.Id);

        pendientes.Select(t => t.Id).ShouldBe(
            new[] { tNuevo.Id, tEspera.Id }, ignoreOrder: false);
        pendientes.Select(t => t.Id).ShouldNotContain(tAsignado.Id);
        pendientes.Select(t => t.Id).ShouldNotContain(tEnAtencion.Id);
        pendientes.Select(t => t.Id).ShouldNotContain(tCerrado.Id);
        pendientes.Select(t => t.Id).ShouldNotContain(tLlamado.Id);
    }

    [Fact]
    public async Task Llamar_DosOperadoresMismoTicket_ElSegundoAvanzaAlSiguiente()
    {
        // Caso 1: dos operadores intentan llamar el mismo ticket. El segundo
        // recibe el siguiente ticket de la cola en lugar de duplicar la llamada.
        var area = await SeedAreaAsync();
        var prioridad = await SeedPrioridadAsync();
        var refs = await SeedReferenciasAsync();
        var ticketA = await SeedTicketAsync(TicketEstado.Nuevo, area, prioridad, "CAJ-001", refs.Servicio, refs.TipoTicket);
        var ticketB = await SeedTicketAsync(TicketEstado.Nuevo, area, prioridad, "CAJ-002", refs.Servicio, refs.TipoTicket);

        var uow = new UnitOfWork(_context);
        var llamar = new LlamarTicketUseCase(uow);

        var primera = await llamar.EjecutarAsync(ticketA.Id, usuarioId: 1, kioskoId: 1);
        primera.TicketId.ShouldBe(ticketA.Id);

        var segunda = await llamar.EjecutarAsync(ticketA.Id, usuarioId: 2, kioskoId: 1);
        segunda.TicketId.ShouldBe(ticketB.Id);

        var pendientes = await uow.Tickets.GetTicketsPendientesPorAreaAsync(area.Id);
        pendientes.ShouldBeEmpty();

        var a = await uow.Tickets.GetByIdAsync(ticketA.Id);
        a!.EstadoTicketId.ShouldBe(TicketEstado.Asignado);
        var b = await uow.Tickets.GetByIdAsync(ticketB.Id);
        b!.EstadoTicketId.ShouldBe(TicketEstado.Asignado);
    }

    [Fact]
    public async Task CerrarSesion_FinalizaAtencionActiva_YCierraSesion()
    {
        // Caso 3: cerrar la sesión (cierre de ventana) finaliza la atención
        // activa y marca el ticket como cerrado.
        var area = await SeedAreaAsync();
        var prioridad = await SeedPrioridadAsync();
        var refs = await SeedReferenciasAsync();
        var ticket = await SeedTicketAsync(TicketEstado.EnAtencion, area, prioridad, "CAJ-001", refs.Servicio, refs.TipoTicket);

        var sesion = new SesionOperador
        {
            UsuarioId = 1,
            PuestoId = 1,
            FechaInicio = DateTime.UtcNow.AddHours(-1)
        };
        _context.SesionesOperador.Add(sesion);
        await _context.SaveChangesAsync();

        var atencion = new Atencion
        {
            TicketId = ticket.Id,
            UsuarioId = 1,
            AreaId = area.Id,
            ServicioId = refs.Servicio.Id,
            EstadoTicketId = TicketEstado.EnAtencion,
            SesionOperadorId = sesion.Id,
            FechaInicio = DateTime.UtcNow.AddMinutes(-5)
        };
        _context.Atenciones.Add(atencion);
        await _context.SaveChangesAsync();

        var uow = new UnitOfWork(_context);
        var cerrar = new CerrarSesionUseCase(uow);

        var result = await cerrar.EjecutarAsync(sesion.Id, "Cierre de ventana");

        result.EstaActiva.ShouldBeFalse();
        result.AtencionesFinalizadas.ShouldBe(1);

        var s = await uow.SesionesOperador.GetByIdAsync(sesion.Id);
        s!.FechaFin.ShouldNotBeNull();

        var a = await uow.Atenciones.GetByIdAsync(atencion.Id);
        a!.FechaFin.ShouldNotBeNull();
        a.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);

        var t = await uow.Tickets.GetByIdAsync(ticket.Id);
        t!.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);
        t.FechaCierre.ShouldNotBeNull();
    }

    [Fact]
    public async Task AbrirSesion_RecuperaOrfanos_CierraSesionPrevia()
    {
        // Caso 3: reabrir sesión después de un cierre abrupto recupera los
        // tickets huérfanos y cierra la sesión anterior.
        var area = await SeedAreaAsync();
        var prioridad = await SeedPrioridadAsync();
        var refs = await SeedReferenciasAsync();
        var ticket = await SeedTicketAsync(TicketEstado.EnAtencion, area, prioridad, "CAJ-001", refs.Servicio, refs.TipoTicket);

        var puesto = new Puesto { AreaId = area.Id, Descripcion = "Puesto 2" };
        _context.Puestos.Add(puesto);
        await _context.SaveChangesAsync();

        var sesionPrevia = new SesionOperador
        {
            UsuarioId = 50,
            PuestoId = 1,
            FechaInicio = DateTime.UtcNow.AddHours(-3)
        };
        _context.SesionesOperador.Add(sesionPrevia);
        await _context.SaveChangesAsync();

        var atencion = new Atencion
        {
            TicketId = ticket.Id,
            UsuarioId = 50,
            AreaId = area.Id,
            ServicioId = refs.Servicio.Id,
            EstadoTicketId = TicketEstado.EnAtencion,
            SesionOperadorId = sesionPrevia.Id,
            FechaInicio = DateTime.UtcNow.AddHours(-1)
        };
        _context.Atenciones.Add(atencion);
        await _context.SaveChangesAsync();

        var uow = new UnitOfWork(_context);
        var abrir = new AbrirSesionUseCase(uow);

        var result = await abrir.EjecutarAsync(new AbrirSesionRequest { UsuarioId = 50, PuestoId = puesto.Id });

        result.EstaActiva.ShouldBeTrue();

        var vieja = await uow.SesionesOperador.GetByIdAsync(sesionPrevia.Id);
        vieja!.FechaFin.ShouldNotBeNull();

        var a = await uow.Atenciones.GetByIdAsync(atencion.Id);
        a!.FechaFin.ShouldNotBeNull();
        a.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);

        var t = await uow.Tickets.GetByIdAsync(ticket.Id);
        t!.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);

        var activa = await uow.SesionesOperador.GetSesionActivaPorUsuarioAsync(50);
        activa.ShouldNotBeNull();
        activa!.Id.ShouldNotBe(sesionPrevia.Id);
    }

    [Fact]
    public async Task Derivar_TicketReingresaComoNuevo_AlAreaDestino()
    {
        // Caso 2: el ticket derivado aparece en la cola del área destino como
        // Nuevo (pendiente por llamar).
        var areaOrigen = await SeedAreaAsync("CAJ", "Caja");
        var areaDestino = await SeedAreaAsync("SAL", "Sala");
        var prioridad = await SeedPrioridadAsync();
        var refs = await SeedReferenciasAsync();

        var ticket = await SeedTicketAsync(TicketEstado.EnAtencion, areaOrigen, prioridad, "CAJ-001", refs.Servicio, refs.TipoTicket);

        _context.UsuariosArea.Add(new UsuarioArea { UsuarioId = 100, AreaId = areaOrigen.Id });
        await _context.SaveChangesAsync();

        var atencion = new Atencion
        {
            TicketId = ticket.Id,
            UsuarioId = 100,
            AreaId = areaOrigen.Id,
            ServicioId = refs.Servicio.Id,
            EstadoTicketId = TicketEstado.EnAtencion,
            FechaInicio = DateTime.UtcNow.AddMinutes(-10)
        };
        _context.Atenciones.Add(atencion);
        await _context.SaveChangesAsync();

        var uow = new UnitOfWork(_context);
        var derivar = new DerivarTicketUseCase(uow);

        await derivar.EjecutarAsync(new DerivarTicketRequest
        {
            TicketId = ticket.Id,
            AtencionId = atencion.Id,
            AreaDestinoId = areaDestino.Id,
            Observacion = "Derivado a Sala"
        });

        var t = await uow.Tickets.GetByIdAsync(ticket.Id);
        t!.AreaActualId.ShouldBe(areaDestino.Id);
        t.EstadoTicketId.ShouldBe(TicketEstado.Nuevo);

        var colaDestino = await uow.Tickets.GetTicketsPendientesPorAreaAsync(areaDestino.Id);
        colaDestino.Select(x => x.Id).ShouldContain(ticket.Id);
    }

    [Fact]
    public async Task CerrarSesion_FinalizaAtencionVencida_Automaticamente()
    {
        // Regla: toda atención con 20+ minutos se finaliza automáticamente.
        // Al cerrar la pestaña (CerrarSesion), la atención ya superó el límite
        // y debe quedar finalizada y el ticket cerrado.
        var area = await SeedAreaAsync();
        var prioridad = await SeedPrioridadAsync();
        var refs = await SeedReferenciasAsync();
        var ticket = await SeedTicketAsync(TicketEstado.EnAtencion, area, prioridad, "CAJ-001", refs.Servicio, refs.TipoTicket);

        var sesion = new SesionOperador
        {
            UsuarioId = 1,
            PuestoId = 1,
            FechaInicio = DateTime.UtcNow.AddMinutes(-30)
        };
        _context.SesionesOperador.Add(sesion);
        await _context.SaveChangesAsync();

        var atencion = new Atencion
        {
            TicketId = ticket.Id,
            UsuarioId = 1,
            AreaId = area.Id,
            ServicioId = refs.Servicio.Id,
            EstadoTicketId = TicketEstado.EnAtencion,
            SesionOperadorId = sesion.Id,
            FechaInicio = DateTime.UtcNow.AddMinutes(-25)
        };
        _context.Atenciones.Add(atencion);
        await _context.SaveChangesAsync();

        var uow = new UnitOfWork(_context);
        var cerrar = new CerrarSesionUseCase(uow);
        var result = await cerrar.EjecutarAsync(sesion.Id, "Cierre de ventana");

        result.EstaActiva.ShouldBeFalse();

        var a = await uow.Atenciones.GetByIdAsync(atencion.Id);
        a!.FechaFin.ShouldNotBeNull();
        a.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);

        var t = await uow.Tickets.GetByIdAsync(ticket.Id);
        t!.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);
        t.FechaCierre.ShouldNotBeNull();
    }

    [Fact]
    public async Task Llamar_AlInicioDelFlujo_FinalizaAtencionesVencidas()
    {
        // Regla: la finalización automática se aplica en todo el flujo, incluso
        // al llamar tickets. Un ticket con 20+ minutos en atención se cierra.
        var area = await SeedAreaAsync();
        var prioridad = await SeedPrioridadAsync();
        var refs = await SeedReferenciasAsync();

        var ticketVencido = await SeedTicketAsync(TicketEstado.EnAtencion, area, prioridad, "CAJ-001", refs.Servicio, refs.TipoTicket);
        var ticketNuevo = await SeedTicketAsync(TicketEstado.Nuevo, area, prioridad, "CAJ-002", refs.Servicio, refs.TipoTicket);

        var atencionVencida = new Atencion
        {
            TicketId = ticketVencido.Id,
            UsuarioId = 1,
            AreaId = area.Id,
            ServicioId = refs.Servicio.Id,
            EstadoTicketId = TicketEstado.EnAtencion,
            FechaInicio = DateTime.UtcNow.AddMinutes(-30)
        };
        _context.Atenciones.Add(atencionVencida);
        await _context.SaveChangesAsync();

        var uow = new UnitOfWork(_context);
        var llamar = new LlamarTicketUseCase(uow);
        await llamar.EjecutarAsync(ticketNuevo.Id, usuarioId: 1, kioskoId: 1);

        var vencido = await uow.Tickets.GetByIdAsync(ticketVencido.Id);
        vencido!.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);
        vencido.FechaCierre.ShouldNotBeNull();

        var a = await uow.Atenciones.GetByIdAsync(atencionVencida.Id);
        a!.FechaFin.ShouldNotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }
}