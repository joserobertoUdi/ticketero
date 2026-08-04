using Ticketero.Api.Mapping;
using Ticketero.Application.DTOs;
using Ticketero.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Ticketero.Api.Services;

public class PdfExportData
{
    public DateTime Inicio { get; set; }
    public DateTime Fin { get; set; }
    public int TotalTickets { get; set; }
    public int TotalAtendidos { get; set; }
    public int TotalPendientes { get; set; }
    public int PromedioAtencion { get; set; }
    public List<PdfAreaStat> StatsPorArea { get; set; } = new();
}

public class PdfAreaStat
{
    public string AreaNombre { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Atendidos { get; set; }
    public int Pendientes { get; set; }
}

public interface IPdfExportService
{
    byte[] GenerateReport(PdfExportData data);
}

public class PdfExportService : IPdfExportService
{
    public byte[] GenerateReport(PdfExportData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text("Sistema Ticketero").SemiBold().FontSize(20).FontColor(Colors.Red.Medium);
                    header.Item().AlignCenter().Text("Reporte de Funcionamiento del Sistema").FontSize(14);
                    header.Item().AlignCenter().Text($"{data.Inicio:dd/MM/yyyy} - {data.Fin:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                    header.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Column(content =>
                {
                    content.Item().PaddingBottom(8).Text("Resumen General").SemiBold().FontSize(14);
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("Total Tickets").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("Atendidos").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("Pendientes").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("Tiempo Prom.").FontSize(10).SemiBold();
                        });

                        table.Cell().Padding(4).Text(data.TotalTickets.ToString());
                        table.Cell().Padding(4).Text(data.TotalAtendidos.ToString());
                        table.Cell().Padding(4).Text(data.TotalPendientes.ToString());
                        table.Cell().Padding(4).Text(MappingService.FormatearTiempo(data.PromedioAtencion));
                    });

                    content.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    content.Item().PaddingBottom(8).Text("Tickets por Area").SemiBold().FontSize(14);
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("Area").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("Total").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("Atendidos").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Red.Lighten4).Padding(4).Text("Pendientes").FontSize(10).SemiBold();
                        });

                        foreach (var stat in data.StatsPorArea)
                        {
                            table.Cell().Padding(4).Text(stat.AreaNombre);
                            table.Cell().Padding(4).Text(stat.Total.ToString());
                            table.Cell().Padding(4).Text(stat.Atendidos.ToString());
                            table.Cell().Padding(4).Text(stat.Pendientes.ToString());
                        }
                    });

                    content.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    content.Item().PaddingBottom(4).Text("Informacion del Sistema").SemiBold().FontSize(14);
                    content.Item().Text($"Periodo analizado: {data.Inicio:dd/MM/yyyy HH:mm} - {data.Fin:dd/MM/yyyy HH:mm}").FontSize(9);
                    content.Item().Text($"Fecha de generacion: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC").FontSize(9);
                    content.Item().Text($"Total de areas: {data.StatsPorArea.Count}").FontSize(9);
                    content.Item().Text($"Porcentaje de atencion: {(data.TotalTickets > 0 ? data.TotalAtendidos * 100 / data.TotalTickets : 0)}%").FontSize(9);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Pagina ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();
    }
}
