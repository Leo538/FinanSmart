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
    private const string SoftBackgroundColor = "#F1F7F8";
    private const string DividerColor = "#DCE8E9";

    public async Task<byte[]> GenerateAsync(CreditSimulationRequestDto request)
    {
        var simulation = await creditSimulationService.SimulateAsync(request);
        var institution = (await institutionService.GetAllAsync()).FirstOrDefault(item => item.IsActive);
        var primaryColor = IsHexColor(institution?.PrimaryColor) ? institution!.PrimaryColor! : DefaultPrimaryColor;
        var displayColor = ReadableBrandColor(primaryColor);
        var logo = await TryLoadLogoAsync(institution?.LogoUrl);

        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.DefaultTextStyle(style => style.FontSize(9));
                page.Header().Element(container => ComposeHeader(container, institution, displayColor, logo));
                page.Content().PaddingVertical(17).Column(column =>
                {
                    column.Spacing(16);
                    ComposeTitle(column, simulation, displayColor);
                    ComposeCreditInformation(column, simulation, displayColor);
                    ComposeFinancialSummary(column, simulation, displayColor);
                    ComposeAppliedCharges(column, simulation, displayColor);
                    column.Item().PageBreak();
                    ComposeAmortizationTable(column, simulation, displayColor);
                });
                page.Footer().Element(container => ComposeFooter(container, institution?.Name));
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, InstitutionDto? institution, string primaryColor, byte[]? logo)
    {
        var name = institution?.Name ?? "FinanSmart";
        container.Background(primaryColor).Padding(17).Row(row =>
        {
            if (logo is not null)
            {
                row.ConstantItem(44).Height(44).Background(Colors.White).Padding(4).Image(logo).FitArea();
                row.ConstantItem(14);
            }
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(name).FontSize(18).Bold().FontColor(Colors.White);
                column.Item().Text("Simulación financiera de crédito").FontSize(9).FontColor("#D8E9EC");
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
        column.Item().Text("SIMULACIÓN DE CRÉDITO").FontSize(17).Bold().FontColor(primaryColor);
        column.Item().Text($"{simulation.CreditTypeName}  ·  {FormatSystem(simulation.AmortizationSystem)}  ·  {simulation.TermMonths} cuotas  ·  Generada el {FormatDate(simulation.GeneratedAt)}").FontSize(9).FontColor("#516B73");
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
        table.Cell().BorderBottom(1).BorderColor(DividerColor).PaddingVertical(9).PaddingHorizontal(12).Column(column =>
        {
            column.Item().Text(label).FontSize(8).FontColor("#617780");
            column.Item().Text(value).SemiBold().FontColor("#173C49");
        });
    }

    private static void ComposeFinancialSummary(ColumnDescriptor column, CreditSimulationResponseDto simulation, string primaryColor)
    {
        column.Item().Text("Resumen financiero").Bold().FontColor(primaryColor);
        column.Item().Border(1).BorderColor(DividerColor).Background(SoftBackgroundColor).Padding(12).Table(table =>
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
        table.Cell().Padding(7).Column(column =>
        {
            column.Item().Text(label).FontSize(8).FontColor("#617780");
            column.Item().Text(value).FontSize(13).Bold().FontColor(primaryColor);
        });
    }

    private static void ComposeAppliedCharges(ColumnDescriptor column, CreditSimulationResponseDto simulation, string primaryColor)
    {
        var charges = simulation.Installments.SelectMany(item => item.Charges)
            .GroupBy(charge => charge.CreditChargeId)
            .Select(group => new { Charge = group.First(), Total = group.Sum(item => item.CalculatedAmount) })
            .ToList();
        if (charges.Count == 0) return;

        column.Item().Text("Seguros y cargos aplicados").Bold().FontColor(primaryColor);
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(2.3f); columns.RelativeColumn(2); columns.RelativeColumn(); columns.RelativeColumn(1.3f); columns.RelativeColumn(1.3f); });
            table.Header(header =>
            {
                header.Cell().Element(cell => HeaderCell(cell, primaryColor)).Text("Concepto");
                header.Cell().Element(cell => HeaderCell(cell, primaryColor)).Text("Tipo");
                header.Cell().Element(cell => HeaderCell(cell, primaryColor)).Text("Frecuencia");
                header.Cell().Element(cell => HeaderCell(cell, primaryColor)).AlignRight().Text("Valor configurado");
                header.Cell().Element(cell => HeaderCell(cell, primaryColor)).AlignRight().Text("Total aplicado");
            });
            for (var index = 0; index < charges.Count; index++)
            {
                var charge = charges[index];
                var rowColor = index % 2 == 0 ? SoftBackgroundColor : "#FFFFFF";
                table.Cell().Element(cell => BodyCell(cell, rowColor)).Text(FormatChargeName(charge.Charge.Name));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).Text(FormatChargeType(charge.Charge.ChargeType));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).Text(FormatFrequency(charge.Charge.Frequency));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(charge.Charge.ChargeType == ChargeType.FixedAmount ? FormatMoney(charge.Charge.ConfiguredValue) : FormatPercentage(charge.Charge.ConfiguredValue));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(FormatMoney(charge.Total)).Bold();
            }
        });
    }

    private static void ComposeAmortizationTable(ColumnDescriptor column, CreditSimulationResponseDto simulation, string primaryColor)
    {
        column.Item().Text("Cronograma de amortización").FontSize(11).Bold().FontColor(primaryColor);
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(31); columns.ConstantColumn(66); columns.RelativeColumn(1.2f); columns.RelativeColumn();
                columns.RelativeColumn(); columns.RelativeColumn(1.2f); columns.RelativeColumn(); columns.RelativeColumn(1.2f); columns.RelativeColumn(1.2f);
            });
            table.Header(header =>
            {
                foreach (var label in new[] { "N.º", "Fecha", "Saldo inicial", "Capital", "Interés", "Cuota financiera", "Cargos", "Pago total", "Saldo final" })
                    header.Cell().Element(cell => HeaderCell(cell, primaryColor)).Text(label);
            });
            var index = 0;
            foreach (var installment in simulation.Installments)
            {
                var rowColor = index++ % 2 == 0 ? SoftBackgroundColor : "#FFFFFF";
                table.Cell().Element(cell => BodyCell(cell, rowColor)).Text(installment.InstallmentNumber.ToString());
                table.Cell().Element(cell => BodyCell(cell, rowColor)).Text(FormatDate(installment.DueDate));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(FormatMoney(installment.OpeningBalance));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(FormatMoney(installment.PrincipalPayment));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(FormatMoney(installment.InterestPayment));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(FormatMoney(installment.BasePayment));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(FormatMoney(installment.AdditionalCharges));
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(FormatMoney(installment.TotalPayment)).Bold();
                table.Cell().Element(cell => BodyCell(cell, rowColor)).AlignRight().Text(FormatMoney(installment.ClosingBalance));
            }
        });
        column.Item().Background(SoftBackgroundColor).Padding(10).Row(row =>
        {
            row.RelativeItem().Text($"Capital: {FormatMoney(simulation.TotalPrincipal)}").Bold().FontColor(primaryColor);
            row.RelativeItem().Text($"Intereses: {FormatMoney(simulation.TotalInterest)}").Bold().FontColor(primaryColor);
            row.RelativeItem().Text($"Cargos: {FormatMoney(simulation.TotalCharges)}").Bold().FontColor(primaryColor);
            row.RelativeItem().AlignRight().Text($"Total: {FormatMoney(simulation.TotalPayment)}").Bold().FontColor(primaryColor);
        });
    }

    private static IContainer HeaderCell(IContainer container, string color) => container.Background(color).PaddingVertical(8).PaddingHorizontal(5).DefaultTextStyle(style => style.Bold().FontSize(8).FontColor(Colors.White));
    private static IContainer BodyCell(IContainer container, string background) => container.Background(background).BorderBottom(1).BorderColor(DividerColor).PaddingVertical(6).PaddingHorizontal(5).DefaultTextStyle(style => style.FontSize(8).FontColor("#294852"));
    private static void ComposeFooter(IContainer container, string? institutionName)
    {
        container.BorderTop(1).BorderColor(DividerColor).PaddingTop(9).AlignCenter().DefaultTextStyle(style => style.FontSize(8).FontColor("#617780")).Text(text =>
        {
            text.Span($"{institutionName ?? "FinanSmart"}  ·  Simulación informativa; los valores pueden variar según las condiciones definitivas.  ·  Página ");
            text.CurrentPageNumber(); text.Span(" de "); text.TotalPages();
        });
    }
    private static string FormatMoney(decimal value) => value.ToString("C2", new System.Globalization.CultureInfo("es-EC"));
    private static string FormatPercentage(decimal value) => $"{value:N2} %";
    private static string FormatDate(DateTimeOffset value) => value.ToString("dd/MM/yyyy");
    private static string FormatSystem(AmortizationSystem system) => system == AmortizationSystem.French ? "Francés" : "Alemán";
    private static string FormatChargeName(string name) => name.Trim().Equals("Installment Protection", StringComparison.OrdinalIgnoreCase) ? "Protección de cuotas" : name;
    private static string FormatChargeType(ChargeType type) => type switch { ChargeType.FixedAmount => "Valor fijo", ChargeType.PercentageOfPrincipal => "Porcentaje sobre capital", _ => "Porcentaje sobre cuota" };
    private static string FormatFrequency(ChargeFrequency frequency) => frequency == ChargeFrequency.Monthly ? "Mensual" : "Una sola vez";
    private static bool IsHexColor(string? value) => value is { Length: 7 } && value[0] == '#' && value[1..].All(Uri.IsHexDigit);
    private static string ReadableBrandColor(string color)
    {
        var red = Convert.ToInt32(color.Substring(1, 2), 16);
        var green = Convert.ToInt32(color.Substring(3, 2), 16);
        var blue = Convert.ToInt32(color.Substring(5, 2), 16);
        return (red * 299 + green * 587 + blue * 114) / 1000 > 160 ? "#0D3747" : color;
    }

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

            const string logoPrefix = "/storage/institution-logos/";
            if (!logoUrl.StartsWith(logoPrefix, StringComparison.OrdinalIgnoreCase)) return null;
            var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "Storage", "institution-logos")) + Path.DirectorySeparatorChar;
            var relativePath = logoUrl[logoPrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
            var path = Path.GetFullPath(Path.Combine(root, relativePath));
            return path.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(path) ? await File.ReadAllBytesAsync(path) : null;
        }
        catch
        {
            return null;
        }
    }
}
