using SolidShowdown.Catalog.Principles;

namespace SolidShowdown.Catalog;

public static class SolidCatalog
{
    public static IReadOnlyList<PrincipleInfo> All { get; } = [Srp.Info, Ocp.Info, Lsp.Info, Isp.Info, Dip.Info];

    public static PrincipleInfo? Find(string slug) =>
        All.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
}
