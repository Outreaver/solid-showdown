namespace SolidShowdown.Examples.Isp.Printers.Bad;

// ❌ BREAKS ISP: a "fat" interface. It describes everything the most capable device can do,
//    instead of what each CLIENT needs.
public interface IMachine
{
    string Print(string document);
    string Scan(string document);
    string Fax(string document);
    string Staple(string document);
}

public sealed class MultiFunctionPrinter : IMachine
{
    public string Print(string document) => $"printed {document}";
    public string Scan(string document) => $"scanned {document}";
    public string Fax(string document) => $"faxed {document}";
    public string Staple(string document) => $"stapled {document}";
}

// ❌ Forced to "implement" three things it can't do. The type system says
//    "this is an IMachine, you can Fax with it" - that's a lie told at compile time
//    and discovered at runtime. (Note the overlap with LSP: these stubs break the contract too.)
public sealed class BasicInkjet : IMachine
{
    public string Print(string document) => $"printed {document}";
    public string Scan(string document) => throw new NotSupportedException();
    public string Fax(string document) => throw new NotSupportedException();
    public string Staple(string document) => throw new NotSupportedException();
}

// ❌ This client only prints, but depends on all four methods. If the Fax signature
//    changes (say, a `resolution` parameter is added), PrintJobService must be recompiled,
//    re-tested and redeployed, and BasicInkjet must be edited although it can't even fax.
public sealed class PrintJobService(IMachine machine)
{
    public string PrintAll(IEnumerable<string> docs) => string.Join("; ", docs.Select(machine.Print));
}
