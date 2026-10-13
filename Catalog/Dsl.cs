namespace SolidShowdown.Catalog;

/// <summary>Terse helpers for declaring diagrams and impacts in the catalog.</summary>
internal static class Dsl
{
    public static Node Cls(string id, double col, double row, string? note = null, string? label = null) =>
        new(id, label ?? id, NodeKind.Class, col, row, note);

    public static Node Ifc(string id, double col, double row, string? note = null, string? label = null) =>
        new(id, label ?? id, NodeKind.Interface, col, row, note);

    public static Node Actor(string id, double col, double row, string? note = null, string? label = null) =>
        new(id, label ?? id, NodeKind.Actor, col, row, note);

    public static Node Ext(string id, double col, double row, string? note = null, string? label = null) =>
        new(id, label ?? id, NodeKind.External, col, row, note);

    public static Edge Uses(string from, string to, string? label = null) => new(from, to, EdgeKind.Uses, label);
    public static Edge Impl(string from, string to) => new(from, to, EdgeKind.Implements);
    public static Edge News(string from, string to, string? label = "new") => new(from, to, EdgeKind.Creates, label);
    public static Edge Asks(string actor, string to) => new(actor, to, EdgeKind.Requests);

    public static Diagram Diagram(Node[] nodes, params Edge[] edges) => new(nodes, edges);

    public static Diagram WithBands(this Diagram diagram, params Band[] bands) => diagram with { Bands = bands };

    public static (string, Impact) Mod(string id) => (id, Impact.Modified);
    public static (string, Impact) Add(string id) => (id, Impact.Added);
    public static (string, Impact) Broke(string id) => (id, Impact.Broken);
    public static (string, Impact) Retest(string id) => (id, Impact.Retest);

    public static VariantImpact Impacts(string summary, params (string, Impact)[] nodes) => VariantImpact.Of(summary, nodes);

    public static string Src(string folder, string file) => $"Examples/{folder}/{file}";
}
