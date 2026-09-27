using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Modules.Credits.Simulations.DTOs;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using FinanSmart.Api.Modules.Institution.DTOs;
using FinanSmart.Api.Modules.Institution.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinanSmart.Api.Modules.Credits.Simulations.Pdf;

public class CreditSimulationPdfService(
    ICreditSimulationService creditSimulationService,
    IInstitutionService institutionService,
    IHttpClientFactory httpClientFactory,
    IWebHostEnvironment environment) : ICreditSimulationPdfService
{
    private const string DefaultPrimaryColor = "#164A5C";

    public async Task<byte[]> GenerateAsync(CreditSimulationRequestDto request)
    {
        var simulation = await creditSimulationService.SimulateAsync(request);
        var institution = (await institutionService.GetAllAsync()).FirstOrDefault(item => item.IsActive);
        var primaryColor = IsHexColor(institution?.PrimaryColor) ? institution!.PrimaryColor! : DefaultPrimaryColor;
        var logo = await TryLoadLogoAsync(institution?.LogoUrl);

        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(style => style.FontSize(9));
                page.Header().Element(container => ComposeHeader(container, institution, primaryColor, logo));
                page.Content().PaddingVertical(14).Column(column =>
                {
                    column.Spacing(14);
                    ComposeTitle(column, simulation, primaryColor);
                    ComposeCreditInformation(column, simulation, primaryColor);
                    ComposeFinancialSummary(column, simulation, primaryColor);
                    ComposeAppliedCharges(column, simulation, primaryColor);
                    ComposeAmortizationTable(column, simulation, primaryColor);
                });
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, InstitutionDto? institution, string primaryColor, byte[]? logo)
    {
        var name = institution?.Name ?? "FinanSmart";
        container.Background(primaryColor).Padding(14).Row(row =>
        {
            if (logo is not null)
            {
                row.ConstantItem(44).Height(44).Background(Colors.White).Padding(4).Image(logo).FitArea();
            }
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(name).FontSize(17).Bold().FontColor(Colors.White);
                column.Item().Text("Simulación financiera de crédito").FontColor(Colors.White);
            });
            row.RelativeItem().AlignRight().Column(column =>
            {
                if (!string.IsNullOrWhiteSpace(institution?.Ruc)) column.Item().Text($"RUC: {institution.Ruc}").FontColor(Colors.White);
                if (!string.IsNullOrWhiteSpace(institution?.Email)) column.Item().Text(institution.Email).FontColor(Colors.White);
            });
        });
    }

    private static void ComposeTitle(ColumnDescriptor column, CreditSimulationResponseDto simulation, string primaryColor)
    {
        column.Item().Text("SIMULACIÓN DE CRÉDITO").FontSize(15).Bold().FontColor(primaryColor);
        column.Item().Text($"Generada el {FormatDate(simulation.GeneratedAt)} · {FormatSystem(simulation.AmortizationSystem)}");
    }

    private static void ComposeCreditInformation(ColumnDescriptor column, CreditSimulationResponseDto simulation, string primaryColor)
    {
        column.Item().Text("Condiciones del crédito").Bold().FontColor(primaryColor);
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); });
            AddInfoCell(table, "Tipo de crédito", simulation.CreditTypeName);
            AddInfoCell(table, "Monto solicitado", FormatMoney(simulation.RequestedAmount));
            AddInfoCell(table, "Plazo", $"{simulation.TermMonths} meses");
            AddInfoCell(table, "Sistema", FormatSystem(simulation.AmortizationSystem));
            AddInfoCell(table, "Tasa anual vigente", FormatPercentage(simulation.AnnualInterestRate));
            AddInfoCell(table, "Tasa mensual equivalente", FormatPercentage(simulation.MonthlyInterestRate));
            AddInfoCell(table, "Fecha de inicio", FormatDate(simulation.StartDate));
        });
    }

    private static void AddInfoCell(TableDescriptor table, string label, string value)
    {
        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(column =>
        {
            column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
            column.Item().Text(value).SemiBold();
        });
    }

    private static void ComposeFinancialSummary(ColumnDescriptor column, CreditSimulationResponseDto simulation, string primaryColor)
    {
        column.Item().Text("Resumen financiero").Bold().FontColor(primaryColor);
        column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(10).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); });
            AddSummaryCell(table, "Capital financiado", FormatMoney(simulation.TotalPrincipal), primaryColor);
            AddSummaryCell(table, "Intereses", FormatMoney(simulation.TotalInterest), primaryColor);
            AddSummaryCell(table, "Cargos", FormatMoney(simulation.TotalCharges), primaryColor);
            AddSummaryCell(table, "Total estimado", FormatMoney(simulation.TotalPayment), primaryColor);
        });
        column.Item().Row(row =>
        {
            row.RelativeItem().Text($"Primera cuota financiera: {FormatMoney(simulation.BaseFirstPayment)}");
            row.RelativeItem().AlignRight().Text($"Última cuota financiera: {FormatMoney(simulation.BaseLastPayment)}");
        });
        var installments = simulation.Installments.ToList();
        if (installments.Count > 0)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Primer pago total con cargos: {FormatMoney(installments[0].TotalPayment)}");
                row.RelativeItem().AlignRight().Text($"Último pago total con cargos: {FormatMoney(installments[^1].TotalPayment)}");
            });
        }
    }

    private static void AddSummaryCell(TableDescriptor table, string label, string value, string primaryColor)
    {
        table.Cell().Padding(4).Column(column =>
        {
            column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
            column.Item().Text(value).Bold().FontColor(primaryColor);
        });
    }

    private static void ComposeAppliedCharges(ColumnDescriptor column, CreditSimulationResponseDto simulation, string primaryColor)
    {
        var charges = simulation.Installments.SelectMany(item => item.Charges)
            .GroupBy(charge => charge.CreditChargeId)
            .Select(group => group.First())
            .ToList();
        if (charges.Count == 0) return;

        column.Item().Text("Cargos incluidos en la proyección").Bold().FontColor(primaryColor);
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(2); columns.RelativeColumn(2); columns.RelativeColumn(); columns.RelativeColumn(); });
            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Nombre");
                header.Cell().Element(HeaderCell).Text("Tipo");
                header.Cell().Element(HeaderCell).Text("Frecuencia");
                header.Cell().Element(HeaderCell).AlignRight().Text("Valor configurado");
            });
            foreach (var charge in charges)
            {
                table.Cell().Element(BodyCell).Text(charge.Name);
                table.Cell().Element(BodyCell).Text(FormatChargeType(charge.ChargeType));
                table.Cell().Element(BodyCell).Text(FormatFrequency(charge.Frequency));
                table.Cell().Element(BodyCell).AlignRight().Text(charge.ChargeType == ChargeType.FixedAmount ? FormatMoney(charge.ConfiguredValue) : FormatPercentage(charge.ConfiguredValue));
            }
        });
    }

    private static void ComposeAmortizationTable(ColumnDescriptor column, CreditSimulationResponseDto simulation, string primaryColor)
    {
        column.Item().Text("Tabla de amortización").Bold().FontColor(primaryColor);
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(22); columns.ConstantColumn(48); columns.RelativeColumn(); columns.RelativeColumn();
                columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn();
            });
            table.Header(header =>
            {
                foreach (var label in new[] { "N.º", "Fecha", "Saldo inicial", "Capital", "Interés", "Cuota financiera", "Cargos", "Pago total", "Saldo final" })
                    header.Cell().Element(HeaderCell).Text(label);
            });
            foreach (var installment in simulation.Installments)
            {
                table.Cell().Element(BodyCell).Text(installment.InstallmentNumber.ToString());
                table.Cell().Element(BodyCell).Text(FormatDate(installment.DueDate));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(installment.OpeningBalance));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(installment.PrincipalPayment));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(installment.InterestPayment));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(installment.BasePayment));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(installment.AdditionalCharges));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(installment.TotalPayment));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(installment.ClosingBalance));
            }
        });
    }

    private static IContainer HeaderCell(IContainer container) => container.Background(Colors.Grey.Lighten2).Padding(4).DefaultTextStyle(style => style.SemiBold().FontSize(7));
    private static IContainer BodyCell(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(2).DefaultTextStyle(style => style.FontSize(7));
    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Darken1)).Text(text =>
        {
            text.Span("Documento generado por FinanSmart. Simulación informativa. Los valores pueden variar según las condiciones definitivas de la operación. ");
            text.CurrentPageNumber(); text.Span(" de "); text.TotalPages();
        });
    }
    private static string FormatMoney(decimal value) => value.ToString("C2", new System.Globalization.CultureInfo("es-EC"));
    private static string FormatPercentage(decimal value) => $"{value:N2} %";
    private static string FormatDate(DateTimeOffset value) => value.ToString("dd/MM/yyyy");
    private static string FormatSystem(AmortizationSystem system) => system == AmortizationSystem.French ? "Francés" : "Alemán";
    private static string FormatChargeType(ChargeType type) => type switch { ChargeType.FixedAmount => "Valor fijo", ChargeType.PercentageOfPrincipal => "Porcentaje sobre capital", _ => "Porcentaje sobre cuota" };
    private static string FormatFrequency(ChargeFrequency frequency) => frequency == ChargeFrequency.Monthly ? "Mensual" : "Una sola vez";
    private static bool IsHexColor(string? value) => value is { Length: 7 } && value[0] == '#' && value[1..].All(Uri.IsHexDigit);

    private async Task<byte[]?> TryLoadLogoAsync(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl)) return null;

        try
        {
            if (logoUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                var separator = logoUrl.IndexOf(',');
                return separator >= 0 ? Convert.FromBase64String(logoUrl[(separator + 1)..]) : null;
            }

            if (Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            {
                using var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode || response.Content.Headers.ContentType?.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) != true)
                    return null;
                return await response.Content.ReadAsByteArrayAsync();
            }

            var root = Path.GetFullPath(environment.ContentRootPath);
            var path = Path.GetFullPath(Path.Combine(root, logoUrl.TrimStart('/', '\\')));
            return path.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(path) ? await File.ReadAllBytesAsync(path) : null;
        }
        catch
        {
            return null;
        }
    }
}
