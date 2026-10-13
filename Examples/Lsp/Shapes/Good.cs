namespace SolidShowdown.Examples.Lsp.Shapes.Good;

// ✅ The abstraction only promises what EVERY shape can honour.
//    Contract: Area >= 0; Scale(k) returns a NEW shape with area * k², original unchanged.
public interface IShape
{
    double Area { get; }
    IShape Scale(double factor);
}

// ✅ Truly immutable: get-only properties, validated once in the constructor. There is no
//    setter (and no `init`) whose postcondition a subtype could betray.
//    (Positional records like `record Rectangle(double Width, double Height)` would generate
//    `init` accessors - see the CONTEXT note below for why that matters.)
public sealed record Rectangle : IShape
{
    public Rectangle(double width, double height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        (Width, Height) = (width, height);
    }

    public double Width { get; }
    public double Height { get; }
    public double Area => Width * Height;
    public IShape Scale(double factor) => new Rectangle(Width * factor, Height * factor);
}

public sealed record Square : IShape
{
    public Square(double side)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(side);
        Side = side;
    }

    public double Side { get; }
    public double Area => Side * Side;
    public IShape Scale(double factor) => new Square(Side * factor);
}

// The banner client needs independent width/height, so it asks for exactly that type:
// it keeps the height and makes the banner four times as wide.
// Passing a Square here is now a COMPILE error instead of a runtime surprise.
public static class BannerLayout
{
    public static Rectangle MakeBanner(Rectangle r) => new(4 * r.Height, r.Height);
}

// 🤔 CONTEXT: LSP is about behaviour (the contract clients rely on), not about taxonomy.
//    Interesting consequence: with a TRULY immutable Rectangle (get-only properties),
//    `Square : Rectangle` would be LSP-compliant - there are no setters whose promises a square could break.
//    C# trap: a positional record has `init` accessors, so `rect with { Width = 4 }` on a Square
//    held as a Rectangle produces a Square whose sides differ. `with` is a setter in disguise.
//    The same "is-a" relationship is valid or invalid depending on what the base type promises.
//    Ask "can every client use the subtype without knowing it?", not "is it a kind of...?".
