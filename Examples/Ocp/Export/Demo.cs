using SolidShowdown.Catalog;
using Bad = SolidShowdown.Examples.Ocp.Export.Bad;
using Good = SolidShowdown.Examples.Ocp.Export.Good;

namespace SolidShowdown.Examples.Ocp.Export;

public static class Demo
{
    public static void RunBad(DemoLog log)
    {
        var report = new Bad.Report("Q3", [new("Widget", 3, 30m), new("Gadget", 1, 99m)]);
        var exporter = new Bad.ReportExporter();

        foreach (var format in exporter.SupportedFormats)
        {
            log.Header(format);
            log.Muted(exporter.Export(report, format).TrimEnd());
        }

        log.Header("Finance asks for XML");
        log.Check("export as xml", () => exporter.Export(report, "xml").Length > 0);
        log.Warn("The only way to fix this is to open ReportExporter.Export and add another `else if`.");
    }

    public static void RunGood(DemoLog log)
    {
        var report = new Good.Report("Q3", [new("Widget", 3, 30m), new("Gadget", 1, 99m)]);

        // This array IS the composition root. Adding XML = adding one item here + one new class.
        Good.IReportFormat[] formats = [new Good.CsvFormat(), new Good.JsonFormat(), new Good.MarkdownFormat(), new Good.XmlFormat()];
        var exporter = new Good.ReportExporter(formats);

        foreach (var format in exporter.SupportedFormats)
        {
            log.Header(format);
            log.Muted(exporter.Export(report, format).TrimEnd());
        }

        log.Header("Finance asks for XML");
        log.Check("export as xml", () => exporter.Export(report, "xml").StartsWith("<report"));
        log.Muted("Delivered by adding XmlFormat - ReportExporter's source is unchanged.");
    }
}
