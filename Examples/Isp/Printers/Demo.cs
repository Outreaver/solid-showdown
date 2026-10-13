using SolidShowdown.Catalog;
using Bad = SolidShowdown.Examples.Isp.Printers.Bad;
using Good = SolidShowdown.Examples.Isp.Printers.Good;

namespace SolidShowdown.Examples.Isp.Printers;

public static class Demo
{
    public static void RunBad(DemoLog log)
    {
        log.Header("What the TYPE SYSTEM promises vs what the object DOES");
        log.Muted("Every method of IMachine is called (via reflection) on each implementation.");
        CapabilityMatrix.Render(log, "IMachine members", [typeof(Bad.IMachine)],
            [new Bad.MultiFunctionPrinter(), new Bad.BasicInkjet()]);

        log.Header("A client that holds an IMachine can't know which calls are safe");
        Bad.IMachine machine = new Bad.BasicInkjet();
        log.Check("machine.Fax(\"contract.pdf\") - compiles fine", () => machine.Fax("contract.pdf").Length > 0);
    }

    public static void RunGood(DemoLog log)
    {
        log.Header("What the TYPE SYSTEM promises vs what the object DOES");
        log.Muted("Only interfaces the class actually implements are called.");
        CapabilityMatrix.Render(log, "Role interfaces",
            [typeof(Good.IPrinter), typeof(Good.IScanner), typeof(Good.IFax), typeof(Good.IStapler)],
            [new Good.MultiFunctionPrinter(), new Good.BasicInkjet()]);

        log.Header("A client that needs to fax must ask for IFax");
        log.Ok("new PrintJobService(new BasicInkjet()) - compiles, PrintJobService only needs IPrinter");
        log.Muted("Passing a BasicInkjet where an IFax is required wouldn't compile: BasicInkjet is not an IFax.");
        log.Info(new Good.PrintJobService(new Good.BasicInkjet()).PrintAll(["a.pdf", "b.pdf"]));
    }
}

/// <summary>Builds a member × implementation table by actually invoking each interface method.</summary>
internal static class CapabilityMatrix
{
    public static void Render(DemoLog log, string caption, Type[] interfaces, object[] implementations)
    {
        var table = log.Table(caption, ["Interface member", .. implementations.Select(i => i.GetType().Name)]);
        var lies = 0;
        foreach (var iface in interfaces)
        foreach (var method in iface.GetMethods())
        {
            var row = new List<DemoCell> { new($"{iface.Name}.{method.Name}()") };
            foreach (var impl in implementations)
            {
                if (!iface.IsInstanceOfType(impl)) { row.Add(new("not claimed", LineKind.Muted)); continue; }
                try
                {
                    method.Invoke(impl, ["doc.pdf"]);
                    row.Add(new("works", LineKind.Ok));
                }
                catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException is NotSupportedException)
                {
                    row.Add(new("claims it, throws", LineKind.Fail));
                    lies++;
                }
            }
            table.Rows.Add([.. row]);
        }
        if (lies == 0) log.Ok("Every claimed capability works.");
        else log.Fail($"{lies} member(s) promised by the interface but not really supported.");
    }
}
