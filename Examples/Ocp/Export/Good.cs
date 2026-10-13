using System.Text;
using System.Text.Json;

namespace SolidShowdown.Examples.Ocp.Export.Good;

public sealed record SalesRow(string Product, int Units, decimal Revenue);

public sealed record Report(string Title, IReadOnlyList<SalesRow> Rows);

// ✅ The extension point. Each format is a plug-in.
public interface IReportFormat
{
    string Id { get; }
    string Render(Report report);
}

// ✅ Closed for modification: it never needs to know which formats exist.
public sealed class ReportExporter(IEnumerable<IReportFormat> formats)
{
    private readonly Dictionary<string, IReportFormat> _formats = formats.ToDictionary(f => f.Id);

    public IReadOnlyCollection<string> SupportedFormats => _formats.Keys;

    public string Export(Report report, string format) =>
        _formats.TryGetValue(format, out var f)
            ? f.Render(report)
            : throw new NotSupportedException($"Unknown format '{format}'");
}

public sealed class CsvFormat : IReportFormat
{
    public string Id => "csv";
    public string Render(Report report)
    {
        var sb = new StringBuilder("product,units,revenue\n");
        foreach (var r in report.Rows) sb.Append($"{r.Product},{r.Units},{r.Revenue}\n");
        return sb.ToString();
    }
}

public sealed class JsonFormat : IReportFormat
{
    public string Id => "json";
    public string Render(Report report) => JsonSerializer.Serialize(report.Rows);
}

public sealed class MarkdownFormat : IReportFormat
{
    public string Id => "md";
    public string Render(Report report)
    {
        var sb = new StringBuilder($"# {report.Title}\n| Product | Units | Revenue |\n|---|---|---|\n");
        foreach (var r in report.Rows) sb.Append($"| {r.Product} | {r.Units} | {r.Revenue} |\n");
        return sb.ToString();
    }
}

// 🤔 CONTEXT: IReportFormat protects us against ONE kind of change: "new output format".
//    It does nothing for "add a Region column to the report" - that changes Report itself,
//    and then every format probably needs an edit. If THAT is the change you keep getting,
//    a better abstraction would be a column model (formats iterate generic columns).
//    You pick the axis based on the change history you actually observe.
