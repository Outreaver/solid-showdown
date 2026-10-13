namespace SolidShowdown.Examples.Lsp.Shapes.Bad;

// The implicit CONTRACT that every client of Rectangle relies on:
//   - after `Width = w`, Width == w and Height is unchanged
//   - after `Height = h`, Height == h and Width is unchanged
//   - Area == Width * Height
public class Rectangle
{
    public virtual double Width { get; set; }
    public virtual double Height { get; set; }
    public double Area => Width * Height;
}

// ❌ BREAKS LSP: "a square IS-A rectangle" is true in geometry, but false for THIS
//    mutable Rectangle's behaviour. To keep its sides equal, Square must secretly change
//    the other dimension - violating the postcondition "Height is unchanged".
//    The compiler is perfectly happy. The breakage is only visible at runtime, in clients.
public class Square : Rectangle
{
    public override double Width
    {
        get => base.Width;
        set { base.Width = value; base.Height = value; }
    }

    public override double Height
    {
        get => base.Height;
        set { base.Width = value; base.Height = value; }
    }
}

// A perfectly reasonable client, written against Rectangle's contract.
public static class BannerLayout
{
    public static double MakeBanner(Rectangle r)
    {
        r.Width = 4;
        r.Height = 1;
        return r.Area; // expects 4 - a Square returns 1
    }
}

// ❌ The classic "fix" makes things worse: clients start type-checking subtypes.
//    if (r is Square) { ... }   <- that's an OCP violation caused by an LSP violation.
//    Every new subtype now means visiting every client.
