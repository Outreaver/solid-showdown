using System.Text;
using System.Text.Json;

namespace SolidShowdown.Examples.Ocp.Export.Bad;

public sealed record SalesRow(string Product, int Units, decimal Revenue);

public sealed record Report(string Title, IReadOnlyList<SalesRow> Rows);

// ❌ BREAKS OCP: the list of formats is hard-coded inside the exporter.
//    Adding XML means editing (and re-testing) a class that already works for CSV/JSON/Markdown.
//    The class also grows forever, and every format's quirks live in one file.
public sealed class ReportExporter
{
    public IReadOnlyList<string> SupportedFormats => ["csv", "json", "md"];

    public string Export(Report report, string format)
    {
        if (format == "csv")
        {
            var sb = new StringBuilder("product,units,revenue\n");
            foreach (var r in report.Rows) sb.Append($"{r.Product},{r.Units},{r.Revenue}\n");
            return sb.ToString();
        }
        else if (format == "json")
        {
            return JsonSerializer.Serialize(report.Rows);
        }
        else if (format == "md")
        {
            var sb = new StringBuilder($"# {report.Title}\n| Product | Units | Revenue |\n|---|---|---|\n");
            foreach (var r in report.Rows) sb.Append($"| {r.Product} | {r.Units} | {r.Revenue} |\n");
            return sb.ToString();
        }

        throw new NotSupportedException($"Unknown format '{format}'"); // ❌ add a format = edit here
    }
}

// 🤔 CONTEXT: if you have 2 formats and no one has asked for a third in years,
//    this if/else is the simplest thing that works. YAGNI beats OCP for things that never change.
//    The refactor to the "open" version is cheap and mechanical - do it when the third format arrives.
