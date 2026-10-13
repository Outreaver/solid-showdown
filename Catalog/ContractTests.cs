namespace SolidShowdown.Catalog;

public static class ContractTests
{
    /// <summary>
    /// Runs every contract rule against a fresh instance of every subject and renders a
    /// rule × subtype matrix. A subtype that fails a rule written for the base type is not substitutable.
    /// </summary>
    public static void RunContract<T>(this DemoLog log, string caption,
        (string Name, Func<T> Create)[] subjects, (string Name, Func<T, bool> Holds)[] contract)
    {
        var table = log.Table(caption, ["Contract rule", .. subjects.Select(s => s.Name)]);
        foreach (var (rule, holds) in contract)
        {
            DemoCell[] row = [new(rule), .. subjects.Select(s => Evaluate(() => holds(s.Create())))];
            table.Rows.Add(row);
        }
        var failures = table.Rows.SelectMany(r => r.Skip(1)).Count(c => c.Kind == LineKind.Fail);
        if (failures == 0) log.Ok($"{caption}: every subtype honours every rule");
        else log.Fail($"{caption}: {failures} rule violation(s) - see the table");
    }

    public static DemoCell Evaluate(Func<bool> check)
    {
        try { return check() ? new("pass", LineKind.Ok) : new("FAIL", LineKind.Fail); }
        catch (Exception ex) { return new($"throws {ex.GetType().Name}", LineKind.Fail); }
    }
}
