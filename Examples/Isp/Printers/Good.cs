namespace SolidShowdown.Examples.Isp.Printers.Good;

// ✅ FOLLOWS ISP: small "role interfaces", each shaped by what some client needs.
public interface IPrinter { string Print(string document); }
public interface IScanner { string Scan(string document); }
public interface IFax { string Fax(string document); }
public interface IStapler { string Staple(string document); }

// A client that genuinely needs the whole bundle can still ask for it -
// interfaces compose. ISP isn't "one method per interface", it's "no client depends on
// methods it doesn't use".
public interface IOfficeDevice : IPrinter, IScanner, IFax, IStapler;

public sealed class MultiFunctionPrinter : IOfficeDevice
{
    public string Print(string document) => $"printed {document}";
    public string Scan(string document) => $"scanned {document}";
    public string Fax(string document) => $"faxed {document}";
    public string Staple(string document) => $"stapled {document}";
}

// ✅ Claims only what it can do. No stubs, no NotSupportedException.
public sealed class BasicInkjet : IPrinter
{
    public string Print(string document) => $"printed {document}";
}

// ✅ Depends on exactly one method. Changes to IFax can't reach it.
public sealed class PrintJobService(IPrinter printer)
{
    public string PrintAll(IEnumerable<string> docs) => string.Join("; ", docs.Select(printer.Print));
}

// 🤔 CONTEXT: where to cut is decided by CLIENTS, not by the implementer.
//    If every client in your system that scans also prints and faxes, then one IScanPrintFax
//    interface is perfectly fine. Splitting into IPrinter/IScanner/... is only worth it because
//    PrintJobService (print only) and, say, ScanToEmail (scan only) exist as separate clients.
//    Too many tiny interfaces is a real cost too: harder discovery, more DI registrations.
