using static SolidShowdown.Catalog.Dsl;
using Accounts_Demo = SolidShowdown.Examples.Lsp.Accounts.Demo;
using Shapes_Demo = SolidShowdown.Examples.Lsp.Shapes.Demo;

namespace SolidShowdown.Catalog.Principles;

internal static class Lsp
{
    public static PrincipleInfo Info => new(
        Slug: "lsp",
        Letter: "L",
        Name: "Liskov Substitution Principle",
        OneLiner: "Subtypes must be usable through the base type without surprises.",
        Definition: "Let φ(x) be a property provable about objects x of type T. Then φ(y) should be true for objects y of type S where S is a subtype of T.",
        DefinitionSource: "Barbara Liskov & Jeannette Wing, A Behavioral Notion of Subtyping (1994), after Liskov's 1987 keynote",
        KeyIdeas:
        [
            "LSP is about behaviour, the contract clients rely on, not about taxonomy (\"is a square a kind of rectangle?\").",
            "A subtype may require less (weaker preconditions) and promise more (stronger postconditions), never the reverse. It must keep the base type's invariants.",
            "Typical violations: throwing where the base doesn't, silently doing less, extra side effects, or changing state the base promised to keep.",
            "The compiler can't catch these. Contract tests can: one test suite written for the base type, run against every subtype.",
        ],
        ItDepends:
        [
            "Whether a subtype violates LSP depends entirely on what the base type promises. Write that contract down (XML docs, tests); otherwise everyone argues about an implicit one.",
            "Square : Rectangle breaks LSP for a mutable Rectangle with independent setters, but is fine for an immutable one. Same \"is-a\", different contract.",
            ".NET deliberately puts capability checks into contracts (ICollection<T>.IsReadOnly, Stream.CanWrite). Throwing NotSupportedException is then legal, but every client has to check.",
            "Whether a fee or a log line counts as a violation depends on whether the contract mentions it. Which side effects are \"allowed\" is a design decision.",
        ],
        Examples: [Shapes, Accounts],
        Quiz:
        [
            new("Which change in a subtype is allowed by LSP?", null,
                ["Throwing from a method the base type implements normally.", "Accepting a wider range of inputs than the base type does.", "Returning values outside the range the base type promised.", "Silently ignoring some calls."],
                1,
                "Weakening a precondition (accepting more) can't surprise an old client: everything it used to pass still works. The other three all break promises the client relied on."),
            new("Does this class violate LSP?",
                "class ReadOnlyBag<T> : Bag<T>\n{\n    public override void Add(T item) => throw new NotSupportedException();\n}",
                ["Always. Throwing in an override is a violation.", "It depends on Bag<T>'s documented contract: if it says Add may throw when IsReadOnly is true (like ICollection<T>), it's compliant.", "Never. NotSupportedException is meant for this.", "Only if Bag<T> is abstract."],
                1,
                "LSP is measured against the contract. ICollection<T> documents NotSupportedException for read-only collections, so List<T>.AsReadOnly() is substitutable, provided clients honour IsReadOnly."),
            new("What is the most reliable warning sign of an LSP violation in client code?", null,
                ["Clients use interfaces instead of classes.", "Clients check `if (x is SomeSubtype)` before calling base-type methods.", "Subtypes override virtual methods.", "The base class is abstract."],
                1,
                "If clients have to know which subtype they hold, the subtypes aren't substitutable. Those type checks then spread with every new subtype, which is how an LSP violation becomes an OCP violation."),
        ]);

    private static Example Shapes => new(
        Id: "shapes",
        Title: "Rectangle & Square: the classic trap",
        Summary: "Geometrically a square is a rectangle. Behaviourally, for a mutable Rectangle whose setters are independent, it isn't. Contract tests make the difference visible.",
        Bad: new(
            [Src("Lsp/Shapes", "Bad.cs")],
            "Square overrides setters and secretly changes the other side",
            "Setting Width on a Square also changes Height. That breaks the postcondition every Rectangle client relies on, and the compiler doesn't notice.",
            Diagram(
                [
                    Cls("BannerLayout", 0.5, 0, "sets W=4, H=1, expects area 4"), Ext("Contract tests", 1.9, 0, "Demo.cs"),
                    Cls("Rectangle", 1.2, 1.3, "virtual Width / Height { get; set; }"),
                    Cls("Square", 1.2, 2.6, "setters change both sides"),
                ],
                Uses("BannerLayout", "Rectangle"), Uses("Contract tests", "Rectangle"), Impl("Square", "Rectangle")),
            Shapes_Demo.RunBad),
        Good: new(
            [Src("Lsp/Shapes", "Good.cs")],
            "An immutable abstraction that only promises what every shape can keep",
            "IShape promises Area and Scale(), and both Rectangle and Square honour them. Code that needs independent width and height asks for Rectangle explicitly.",
            Diagram(
                [
                    Cls("BannerLayout", 0, 0, "needs width & height"), Ext("Contract tests", 1.9, 0, "Demo.cs"),
                    Ifc("IShape", 1.2, 1.3, "Area · Scale(k)"),
                    Cls("Rectangle", 0.5, 2.6, "record (Width, Height)"), Cls("Square", 1.9, 2.6, "record (Side)"),
                ],
                Uses("BannerLayout", "Rectangle"), Uses("Contract tests", "IShape"),
                Impl("Rectangle", "IShape"), Impl("Square", "IShape")),
            Shapes_Demo.RunGood),
        Scenarios:
        [
            new("Someone passes a Square to BannerLayout", "Any developer",
                "The method signature says Rectangle, so why not?",
                Impacts("It compiles and runs, and returns area 1 instead of 4. The bug shows up far from its cause.",
                    Broke("BannerLayout"), Broke("Square")),
                Impacts("It doesn't compile. Square isn't a Rectangle, so the mistake is caught immediately."),
                "LSP violations are runtime surprises hidden behind a type system that says everything is fine."),
            new("\"Fix\" the banner bug", "Any developer",
                "Make BannerLayout work for squares.",
                Impacts("BannerLayout grows an `if (r is Square)` branch, and so does every other Rectangle client. Each new subtype adds more of these.",
                    Mod("BannerLayout")),
                Impacts("Nothing to fix."),
                "Type checks in clients are the classic symptom of an LSP violation, and they turn it into an OCP violation as well."),
            new("Add a Circle", "Product",
                "The gallery should support circles.",
                Impacts("Circle can't sensibly be a Rectangle (what is its Width setter?). The hierarchy has to be reshaped.",
                    Mod("Rectangle")).Adding([Cls("Circle", 2.4, 2.6, "Width = Height = diameter?")], Impl("Circle", "Rectangle")),
                Impacts("A new record that implements IShape, verified by the same contract tests.")
                    .Adding([Cls("Circle", 3.1, 2.6, "record (Radius)")], Impl("Circle", "IShape")),
                "A contract that promises only what all subtypes share is easier to extend, so LSP helps OCP."),
        ],
        DemoSourcePath: Src("Lsp/Shapes", "Demo.cs"),
        ContextNotes:
        [
            "With an immutable Rectangle (no setters), Square : Rectangle would pass every contract test. The is-a relation is only valid relative to a contract.",
            "Contract tests (one suite run against all implementations) are the most practical LSP tool. The demos use exactly that technique.",
        ]);

    private static Example Accounts => new(
        Id: "accounts",
        Title: "Bank accounts: preconditions & postconditions",
        Summary: "A fixed-term deposit that refuses withdrawals (strengthened precondition) and an account that takes a hidden fee (weakened postcondition). Then the same requirements done with an explicit contract.",
        Bad: new(
            [Src("Lsp/Accounts", "Bad.cs")],
            "Subclasses quietly change what Withdraw() means",
            "Account promises that withdrawing up to the balance works and lowers the balance by exactly that amount. Two subclasses break that promise, one loudly and one silently.",
            Diagram(
                [
                    Cls("MonthlyFeeJob", 1, 0, "Withdraw(fee) on every account"),
                    Cls("Account", 1, 1.3, "Withdraw: Balance −= amount"),
                    Cls("CheckingAccount", 0, 2.6), Cls("FixedTermDeposit", 1, 2.6, "Withdraw throws"), Cls("PayAsYouGoAccount", 2, 2.6, "takes hidden 1.00 fee"),
                ],
                Uses("MonthlyFeeJob", "Account"),
                Impl("CheckingAccount", "Account"), Impl("FixedTermDeposit", "Account"), Impl("PayAsYouGoAccount", "Account")),
            Accounts_Demo.RunBad),
        Good: new(
            [Src("Lsp/Accounts", "Good.cs")],
            "Withdrawing is an explicit capability with an explicit contract",
            "Only accounts that can withdraw implement IWithdrawable, and fees and overdraft limits are part of its contract, so subtypes can vary without surprising anyone.",
            Diagram(
                [
                    Cls("MonthlyFeeJob", 0.5, 0),
                    Ifc("IWithdrawable", 0.5, 1.3, "FeeFor · OverdraftLimit · Withdraw"), Cls("Account", 2.3, 1.3, "abstract: Balance · Deposit"),
                    Cls("CheckingAccount", 1, 2.6), Cls("FixedTermDeposit", 2.6, 2.6, "no Withdraw at all"),
                    Cls("PayAsYouGoAccount", 0.3, 3.9, "FeeFor = 1.00"), Cls("OverdraftAccount", 1.7, 3.9, "OverdraftLimit in the contract"),
                ],
                Uses("MonthlyFeeJob", "IWithdrawable"),
                Impl("CheckingAccount", "IWithdrawable"), Impl("CheckingAccount", "Account"), Impl("FixedTermDeposit", "Account"),
                Impl("PayAsYouGoAccount", "CheckingAccount"), Impl("OverdraftAccount", "CheckingAccount")),
            Accounts_Demo.RunGood),
        Scenarios:
        [
            new("Run the monthly fee job", "Operations",
                "Charge 5.00 to every account.",
                Impacts("The job crashes on the FixedTermDeposit after charging some accounts but not others. Now you have a half-applied batch.",
                    Broke("MonthlyFeeJob"), Broke("FixedTermDeposit")),
                Impacts("FixedTermDeposit isn't IWithdrawable, so it can't even be passed to the job."),
                "A strengthened precondition (\"I accept nothing\") breaks every client that trusted the base contract."),
            new("Add transfers between accounts", "Product",
                "Withdraw from A, deposit into B.",
                Impacts("Each transfer from a PayAsYouGo account makes 1.00 disappear. No exception, and the books don't balance.",
                    Broke("PayAsYouGoAccount")).Adding([Cls("TransferService", 2.3, 0)], Uses("TransferService", "Account")),
                Impacts("The fee is part of the contract (FeeFor), so the transfer can show it and book it explicitly.")
                    .Adding([Cls("TransferService", 1.7, 0)], Uses("TransferService", "IWithdrawable")),
                "A weakened postcondition is the nastier violation, because nothing fails loudly."),
            new("Savings: max 3 withdrawals a month", "Product",
                "The 4th withdrawal in a month must be refused.",
                Impacts("SavingsAccount : Account throws on the 4th call. That's a strengthened precondition in disguise, and the fee job will crash again some day.",
                    Retest("MonthlyFeeJob")).Adding([Cls("SavingsAccount", 3, 2.6, "4th Withdraw throws")], Impl("SavingsAccount", "Account")),
                Impacts("A conscious contract decision: either add CanWithdraw(amount) to IWithdrawable and let clients check, or don't make savings IWithdrawable.",
                    Mod("IWithdrawable"), Retest("MonthlyFeeJob")).Adding([Cls("SavingsAccount", 3.6, 2.6, "honours the new contract")], Impl("SavingsAccount", "Account")),
                "Every \"…except when\" in a subtype is a question about the contract. Answer it in the contract, not in a surprise exception."),
        ],
        DemoSourcePath: Src("Lsp/Accounts", "Demo.cs"),
        ContextNotes:
        [
            "If Account had documented \"balance decreases by amount plus applicable fees\", PayAsYouGoAccount would not be a violation. Contracts decide.",
            "OverdraftAccount and PayAsYouGoAccount behave differently from CheckingAccount and still satisfy LSP, because IWithdrawable states its rules in terms of OverdraftLimit and FeeFor and never promises a non-negative balance. A client that assumed Balance >= 0 would be relying on something the contract doesn't say.",
        ]);
}
