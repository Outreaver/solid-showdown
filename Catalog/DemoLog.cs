namespace SolidShowdown.Catalog;

public enum LineKind { Info, Header, Ok, Fail, Warn, Muted }

public sealed record DemoLine(LineKind Kind, string Text);

public sealed record DemoCell(string Text, LineKind Kind = LineKind.Info);

public sealed record DemoTable(string Caption, string[] Headers, List<DemoCell[]> Rows);

/// <summary>
/// What a demo writes to. The UI renders it as a small console, plus optional tables
/// (used e.g. for the LSP contract results and the ISP "who implements what" matrix).
/// </summary>
public sealed class DemoLog
{
    public List<DemoLine> Lines { get; } = [];
    public List<DemoTable> Tables { get; } = [];

    public void Header(string text) => Lines.Add(new(LineKind.Header, text));
    public void Info(string text) => Lines.Add(new(LineKind.Info, text));
    public void Ok(string text) => Lines.Add(new(LineKind.Ok, "✔ " + text));
    public void Fail(string text) => Lines.Add(new(LineKind.Fail, "✘ " + text));
    public void Warn(string text) => Lines.Add(new(LineKind.Warn, "⚠ " + text));
    public void Muted(string text) => Lines.Add(new(LineKind.Muted, text));

    /// <summary>Runs a check and logs a pass or fail line. Exceptions count as failures.</summary>
    public bool Check(string description, Func<bool> check)
    {
        try
        {
            if (check()) { Ok(description); return true; }
            Fail(description);
            return false;
        }
        catch (Exception ex)
        {
            Fail($"{description} — threw {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    public DemoTable Table(string caption, params string[] headers)
    {
        var table = new DemoTable(caption, headers, []);
        Tables.Add(table);
        return table;
    }
}
