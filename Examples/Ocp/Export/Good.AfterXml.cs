using System.Xml.Linq;

namespace SolidShowdown.Examples.Ocp.Export.Good;

// Request: "Finance needs XML."
// ✅ New class. ReportExporter, CsvFormat, JsonFormat, MarkdownFormat: untouched.
public sealed class XmlFormat : IReportFormat
{
    public string Id => "xml";

    public string Render(Report report) =>
        new XElement("report",
            new XAttribute("title", report.Title),
            report.Rows.Select(r => new XElement("row",
                new XAttribute("product", r.Product),
                new XAttribute("units", r.Units),
                new XAttribute("revenue", r.Revenue)))).ToString(SaveOptions.DisableFormatting);
}

// ...and one line in the composition root (Program.cs / DI setup):
//     services.AddSingleton<IReportFormat, XmlFormat>();
// That edit is in "wiring" code, which is expected to change. Business code stays closed.
