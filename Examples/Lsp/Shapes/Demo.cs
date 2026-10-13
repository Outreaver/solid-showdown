using SolidShowdown.Catalog;
using Bad = SolidShowdown.Examples.Lsp.Shapes.Bad;
using Good = SolidShowdown.Examples.Lsp.Shapes.Good;

namespace SolidShowdown.Examples.Lsp.Shapes;

// A "contract test": ONE test suite written against the base type, executed against
// every subtype. If a subtype fails it, that subtype is not substitutable. This is the
// most practical way to check LSP in real code.
public static class Demo
{
    public static void RunBad(DemoLog log)
    {
        (string Name, Func<Bad.Rectangle> Create)[] subjects =
            [("Rectangle", () => new Bad.Rectangle()), ("Square", () => new Bad.Square())];

        (string Name, Func<Bad.Rectangle, bool> Holds)[] contract =
        [
            ("Setting Width keeps Height", r => { r.Height = 2; r.Width = 5; return r.Height == 2; }),
            ("Setting Height keeps Width", r => { r.Width = 5; r.Height = 2; return r.Width == 5; }),
            ("Setting Width = 5, then Height = 2 gives Area 10", r => { r.Width = 5; r.Height = 2; return r.Area == 10; }),
        ];

        log.RunContract("Rectangle contract", subjects, contract);

        log.Header("A client written against Rectangle");
        foreach (var (name, create) in subjects)
        {
            var area = Bad.BannerLayout.MakeBanner(create());
            log.Check($"BannerLayout.MakeBanner({name}) == 4 (got {area})", () => area == 4);
        }
    }

    public static void RunGood(DemoLog log)
    {
        (string Name, Func<Good.IShape> Create)[] subjects =
            [("Rectangle", () => new Good.Rectangle(5, 2)), ("Square", () => new Good.Square(3))];

        (string Name, Func<Good.IShape, bool> Holds)[] contract =
        [
            ("Area is non-negative", s => s.Area >= 0),
            ("Scale(2) quadruples the area", s => Math.Abs(s.Scale(2).Area - s.Area * 4) < 1e-9),
            ("Scale leaves the original unchanged", s => { var before = s.Area; s.Scale(3); return s.Area == before; }),
        ];

        log.RunContract("IShape contract", subjects, contract);

        log.Header("A client written against Rectangle");
        var area = Good.BannerLayout.MakeBanner(new Good.Rectangle(2, 1)).Area;
        log.Check($"BannerLayout.MakeBanner(Rectangle).Area == 4 (got {area})", () => area == 4);
        log.Muted("BannerLayout.MakeBanner(new Square(3)) wouldn't compile, so it can't misbehave.");
    }
}
