namespace SolidShowdown.Catalog;

public sealed record PrincipleInfo(
    string Slug,
    string Letter,
    string Name,
    string OneLiner,
    string Definition,
    string DefinitionSource,
    string[] KeyIdeas,
    string[] ItDepends,
    IReadOnlyList<Example> Examples,
    IReadOnlyList<QuizQuestion> Quiz);

public sealed record Example(
    string Id,
    string Title,
    string Summary,
    Variant Bad,
    Variant Good,
    IReadOnlyList<ChangeScenario> Scenarios,
    string DemoSourcePath,
    string[] ContextNotes);

/// <summary>One side of the comparison: the design that breaks the principle, or the one that follows it.</summary>
public sealed record Variant(
    string[] SourcePaths,
    string Headline,
    string Explanation,
    Diagram Diagram,
    Action<DemoLog> Run);

// ---------- Structure diagrams ----------

public enum NodeKind { Class, Interface, Actor, External }

public enum EdgeKind
{
    /// <summary>Calls / holds a reference to.</summary>
    Uses,
    /// <summary>Implements an interface or derives from a base class.</summary>
    Implements,
    /// <summary>Instantiates with <c>new</c> - a hard-wired dependency.</summary>
    Creates,
    /// <summary>An actor (a person or team) who asks for changes to this unit.</summary>
    Requests,
}

/// <summary>A box on the diagram. <paramref name="Col"/> / <paramref name="Row"/> are grid coordinates (fractions allowed).</summary>
public sealed record Node(string Id, string Label, NodeKind Kind, double Col, double Row, string? Note = null);

public sealed record Edge(string From, string To, EdgeKind Kind, string? Label = null);

public sealed record Diagram(IReadOnlyList<Node> Nodes, IReadOnlyList<Edge> Edges)
{
    /// <summary>Optional labelled horizontal regions, e.g. "Domain" vs "Infrastructure".</summary>
    public IReadOnlyList<Band> Bands { get; init; } = [];
}

/// <summary>A labelled region. Columns default to the full diagram width.</summary>
public sealed record Band(string Label, double FromRow, double ToRow, double? FromCol = null, double? ToCol = null);

// ---------- Change scenarios ("what happens when someone asks for X?") ----------

public enum Impact
{
    /// <summary>Code that has to be edited to fulfil the request. Expected.</summary>
    Modified,
    /// <summary>New code - nothing existing is touched.</summary>
    Added,
    /// <summary>Code that changes behaviour although nobody asked for it to. This is the real cost of the violation.</summary>
    Broken,
    /// <summary>Not edited, but recompiled/redeployed/re-tested because it lives with or depends on edited code.</summary>
    Retest,
}

public sealed record ChangeScenario(
    string Title,
    string RequestedBy,
    string Description,
    VariantImpact Bad,
    VariantImpact Good,
    string Lesson);

public sealed record VariantImpact(
    string Summary,
    IReadOnlyDictionary<string, Impact> Nodes,
    IReadOnlyList<Node> NewNodes,
    IReadOnlyList<Edge> NewEdges)
{
    public static VariantImpact Of(string summary, params (string Node, Impact Impact)[] nodes) =>
        new(summary, nodes.ToDictionary(n => n.Node, n => n.Impact), [], []);

    /// <summary>Adds boxes/arrows that only exist after the change (they are drawn as "Added").</summary>
    public VariantImpact Adding(Node[] nodes, params Edge[] edges) =>
        this with { NewNodes = [.. NewNodes, .. nodes], NewEdges = [.. NewEdges, .. edges] };
}

// ---------- Quiz ----------

public sealed record QuizQuestion(string Question, string? Code, string[] Options, int CorrectIndex, string Explanation);
