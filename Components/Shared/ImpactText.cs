using SolidShowdown.Catalog;

namespace SolidShowdown.Components.Shared;

public static class ImpactText
{
    public static string Label(Impact impact) => impact switch
    {
        Impact.Modified => "modified",
        Impact.Added => "added",
        Impact.Broken => "broken / surprised",
        _ => "re-test / redeploy",
    };

    public static string Describe(Impact impact) => impact switch
    {
        Impact.Modified => "Edited to fulfil the request (expected).",
        Impact.Added => "New code; nothing existing is touched.",
        Impact.Broken => "Changes behaviour although nobody asked it to. This is the real cost.",
        _ => "Not edited, but must be re-verified or redeployed because it lives with or depends on edited code.",
    };

    /// <summary>"2 modified · 1 broken" style summary for a scenario side.</summary>
    public static string Counts(VariantImpact impact)
    {
        var counts = impact.Nodes.Values
            .Concat(impact.NewNodes.Where(n => !impact.Nodes.ContainsKey(n.Id)).Select(_ => Impact.Added))
            .GroupBy(i => i)
            .OrderBy(g => g.Key switch { Impact.Broken => 0, Impact.Modified => 1, Impact.Retest => 2, _ => 3 })
            .Select(g => $"{g.Count()} {Label(g.Key)}");
        var text = string.Join(" · ", counts);
        return text.Length == 0 ? "nothing touched" : text;
    }
}
