namespace SolidShowdown.Examples.Lsp.Shapes.Good;

// ✅ The abstraction only promises what EVERY shape can honour.
//    Contract: Area >= 0; Scale(k) returns a NEW shape with area * k², original unchanged.
public interface IShape
{
    double Area { get; }
    IShape Scale(double factor);
}

// ✅ Immutable: there is no "set Width" whose postcondition a subtype could betray.
public sealed record Rectangle(double Width, double Height) : IShape
{
    public double Area => Width * Height;
    public IShape Scale(double factor) => new Rectangle(Width * factor, Height * factor);
    public Rectangle WithSize(double width, double height) => new(width, height);
}

public sealed record Square(double Side) : IShape
{
    public double Area => Side * Side;
    public IShape Scale(double factor) => new Square(Side * factor);
}

// The banner client needs independent width/height, so it asks for exactly that type.
// Passing a Square here is now a COMPILE error instead of a runtime surprise.
public static class BannerLayout
{
    public static double MakeBanner(Rectangle r) => r.WithSize(4, 1).Area;
}

// 🤔 CONTEXT: LSP is about behaviour (the contract clients rely on), not about taxonomy.
//    Interesting consequence: with an IMMUTABLE Rectangle, `Square : Rectangle` would be
//    perfectly LSP-compliant - there are no setters whose promises a square could break.
//    The same "is-a" relationship is valid or invalid depending on what the base type promises.
//    Ask "can every client use the subtype without knowing it?", not "is it a kind of...?".
