using static SolidShowdown.Catalog.Dsl;
using Discounts_Demo = SolidShowdown.Examples.Ocp.Discounts.Demo;
using Export_Demo = SolidShowdown.Examples.Ocp.Export.Demo;

namespace SolidShowdown.Catalog.Principles;

internal static class Ocp
{
    public static PrincipleInfo Info => new(
        Slug: "ocp",
        Letter: "O",
        Name: "Open/Closed Principle",
        OneLiner: "Open for extension, closed for modification.",
        Definition: "Software entities should be open for extension, but closed for modification.",
        DefinitionSource: "Bertrand Meyer, Object-Oriented Software Construction (1988); popularised in its polymorphic form by Robert C. Martin",
        KeyIdeas:
        [
            "Adding new behaviour shouldn't require editing code that already works and is already tested.",
            "In practice: put the part that varies behind an abstraction (an interface, a strategy, a plug-in) and add new implementations.",
            "The smell to watch for is the same switch or if/else on a type code repeated in several places (Fowler's \"Repeated Switches\"). Each new case then means shotgun surgery.",
            "Modification doesn't disappear. It moves to the edges, into the composition root or DI registration, away from the business rules.",
        ],
        ItDepends:
        [
            "No module is closed against every change. An abstraction is closed against the kinds of change it anticipated, and any other kind forces you to modify it.",
            "Guessing every future axis gives you speculative generality: abstractions nobody needed that make the code harder to read.",
            "\"Fool me once\": write the simple version first. When the first change of a given kind arrives, refactor so the next change of that kind is an extension.",
            "A switch over a stable, closed set (days of the week, HTTP methods you support) is not a violation. The question is whether the business keeps extending that set.",
            "Changing an existing rule (premium discount 10% → 12%) is a modification by nature, and that's fine. OCP is about adding behaviour.",
        ],
        Examples: [Discounts, Export],
        Quiz:
        [
            new("Which statement about OCP is most accurate?", null,
                ["Once a class is written it must never be edited again.", "You should be able to add anticipated kinds of new behaviour without editing existing, working code.", "Every class needs an interface.", "Always prefer inheritance over composition."],
                1,
                "OCP is about the kinds of change you expect. Bug fixes and changes to existing rules are still edits. OCP says new variants shouldn't require reopening working code."),
            new("Is this switch an OCP violation?",
                "TimeSpan OpeningHours(DayOfWeek day) => day switch\n{\n    DayOfWeek.Saturday or DayOfWeek.Sunday => TimeSpan.FromHours(6),\n    _ => TimeSpan.FromHours(10),\n};",
                ["Yes, every switch violates OCP.", "Probably not. The set of cases is closed and stable; nobody will add an 8th day.", "Yes, it should be a Strategy.", "Only if it isn't sealed."],
                1,
                "OCP targets axes of change that keep growing. A switch over a closed set is often the clearest code you can write."),
            new("You built ICustomerPricing.Price(decimal basePrice). Marketing now wants each customer kind to get its own discount based on the whole cart. What does that tell you?", null,
                ["You applied OCP incorrectly.", "This is a kind of change the abstraction wasn't closed against. Modify it, and ask whether this axis will keep coming back.", "Never touch the interface; add a second, parallel one.", "Switch to reflection-based plug-ins."],
                1,
                "This is OCP's honest limit: closure is strategic. You protected against new customer kinds, not against new inputs to pricing. Modify it now, and you've learned which axis matters."),
        ]);

    private static Example Discounts => new(
        Id: "discounts",
        Title: "Customer discounts: switch vs. strategy",
        Summary: "Pricing and loyalty points vary by customer kind. The switch-based version needs edits in several places for each new kind. The strategy version needs one new class, and it shows what the abstraction can't protect against.",
        Bad: new(
            [Src("Ocp/Discounts", "Bad.cs"), Src("Ocp/Discounts", "Bad.AfterStudent.cs")],
            "A type code switched on in more than one class",
            "Adding a customer kind means editing the enum and every switch over it. Because each switch ends in a catch-all `_ =>` case, the compiler can't tell you which one you forgot.",
            Diagram(
                [
                    Cls("PriceCalculator", 0, 0.4, "switch (CustomerType)"), Cls("LoyaltyCalculator", 2, 0.4, "switch (CustomerType)"),
                    Cls("CustomerType", 1, 1.7, "Regular · Premium · Staff", "enum CustomerType"),
                ],
                Uses("PriceCalculator", "CustomerType"), Uses("LoyaltyCalculator", "CustomerType")),
            Discounts_Demo.RunBad),
        Good: new(
            [Src("Ocp/Discounts", "Good.cs"), Src("Ocp/Discounts", "Good.AfterStudent.cs")],
            "Everything that varies per customer kind sits behind one interface",
            "A new kind of customer is a new class. Checkout never changes, and the interface makes the compiler require every per-kind decision.",
            Diagram(
                [
                    Cls("Checkout", 1, 0),
                    Ifc("ICustomerPricing", 1, 1.3, "Price · LoyaltyPoints"),
                    Cls("RegularPricing", 0, 2.6), Cls("PremiumPricing", 1, 2.6), Cls("StaffPricing", 2, 2.6),
                ],
                Uses("Checkout", "ICustomerPricing"),
                Impl("RegularPricing", "ICustomerPricing"), Impl("PremiumPricing", "ICustomerPricing"), Impl("StaffPricing", "ICustomerPricing")),
            Discounts_Demo.RunGood),
        Scenarios:
        [
            new("Add Student customers", "Product",
                "Students get 15% off and 1 loyalty point per unit spent.",
                Impacts("The enum and PriceCalculator are edited. LoyaltyCalculator is forgotten; it still compiles, and students silently get 0 points.",
                    Mod("CustomerType"), Mod("PriceCalculator"), Broke("LoyaltyCalculator")),
                Impacts("One new class. No existing file is opened, and the interface forces a decision about loyalty points.")
                    .Adding([Cls("StudentPricing", 3, 2.6)], Impl("StudentPricing", "ICustomerPricing")),
                "This is the axis the design was built for, and adding a variant is purely additive. Run the demos to see the forgotten case."),
            new("Per-kind cart discounts", "Marketing",
                "Premium customers who spend over 500 get an extra 10% off, students over 200 get 5%. Each kind's discount now depends on the whole cart.",
                Impacts("PriceCalculator's switch needs the cart. Its signature and every caller change.",
                    Mod("PriceCalculator")),
                Impacts("The rule varies per customer kind, so it belongs in ICustomerPricing, but Price(decimal basePrice) doesn't receive the cart. The interface, every implementation and Checkout must change.",
                    Mod("ICustomerPricing"), Mod("RegularPricing"), Mod("PremiumPricing"), Mod("StaffPricing"), Mod("Checkout")),
                "This is OCP's limit made visible: the \"good\" design is closed against new customer kinds, not against new pricing inputs. (A flat cart discount for everyone wouldn't touch the interface. It could be a separate rule in Checkout.) No design can be open to every extension."),
            new("Premium discount 10% → 12%", "Marketing",
                "An existing rule changes.",
                Impacts("One line in PriceCalculator.", Mod("PriceCalculator")),
                Impacts("One line in PremiumPricing.", Mod("PremiumPricing")),
                "Changing an existing rule is a modification, and that's expected. OCP is about adding behaviour without reopening working code, not about never editing anything."),
        ],
        DemoSourcePath: Src("Ocp/Discounts", "Demo.cs"),
        ContextNotes:
        [
            "If you've had the same three tiers for years and nobody asks for more, the switch is simpler and perfectly fine.",
            "Something still has to decide which ICustomerPricing a customer gets: a factory, a DI registration or a database column. That's the composition root, and it changes. That's OK.",
        ]);

    private static Example Export => new(
        Id: "export",
        Title: "Report export: if/else vs. plug-in formats",
        Summary: "An exporter with a growing if/else chain of formats, versus a registry of IReportFormat plug-ins. Adding XML shows the difference; adding a column shows the limit.",
        Bad: new(
            [Src("Ocp/Export", "Bad.cs")],
            "Formats are hard-coded inside the exporter",
            "Every new format reopens ReportExporter.Export(), a method that already works for three formats, and makes it longer.",
            Diagram(
                [
                    Cls("ReportExporter", 1, 0.4, "if csv · else if json · else if md"),
                    Cls("Report", 1, 1.7, "Title · Rows"),
                ],
                Uses("ReportExporter", "Report")),
            Export_Demo.RunBad),
        Good: new(
            [Src("Ocp/Export", "Good.cs"), Src("Ocp/Export", "Good.AfterXml.cs")],
            "Each format is a plug-in; the exporter just looks them up",
            "ReportExporter receives IEnumerable<IReportFormat> and never needs to know which formats exist. Only the composition root does.",
            Diagram(
                [
                    Ext("Composition root", 0, 0, "Program.cs / DI"), Cls("ReportExporter", 1, 0),
                    Ifc("IReportFormat", 1, 1.3, "Id · Render(Report)"), Cls("Report", 2.4, 1.3, "Title · Rows"),
                    Cls("CsvFormat", 0, 2.6), Cls("JsonFormat", 1, 2.6), Cls("MarkdownFormat", 2, 2.6),
                ],
                News("Composition root", "ReportExporter", "new + formats"),
                Uses("ReportExporter", "IReportFormat"), Uses("IReportFormat", "Report"),
                Impl("CsvFormat", "IReportFormat"), Impl("JsonFormat", "IReportFormat"), Impl("MarkdownFormat", "IReportFormat")),
            Export_Demo.RunGood),
        Scenarios:
        [
            new("Finance needs XML", "Finance",
                "Add an XML export.",
                Impacts("Another else-if in ReportExporter.Export(). The existing formats live in the same method and need re-testing.",
                    Mod("ReportExporter")),
                Impacts("A new XmlFormat class plus one registration line in the composition root. The exporter and other formats are untouched.",
                    Mod("Composition root")).Adding([Cls("XmlFormat", 3, 2.6)], Impl("XmlFormat", "IReportFormat")),
                "Business code stays closed. The edit moves to wiring code, which is expected to change."),
            new("Add a Region column", "Sales",
                "Every report row gets a Region.",
                Impacts("Report changes, and so do the CSV and Markdown branches of Export(). The JSON branch serialises the new field automatically, but it sits in the same method, so the whole exporter is re-tested.",
                    Mod("Report"), Mod("ReportExporter")),
                Impacts("Report changes, and so do the hand-written CSV and Markdown formats. JSON serialises rows automatically, but should be re-checked.",
                    Mod("Report"), Mod("CsvFormat"), Mod("MarkdownFormat"), Retest("JsonFormat")),
                "IReportFormat protects against new formats, not against new data. If columns are what keeps changing, a generic column model would be the better abstraction."),
            new("CSV must use ';' for Excel", "Finance",
                "Change the CSV delimiter.",
                Impacts("Edit inside the shared Export() method, next to JSON and Markdown.", Mod("ReportExporter")),
                Impacts("Edit in CsvFormat only.", Mod("CsvFormat")),
                "A pleasant side effect: each variant's quirks live in their own small class, so OCP and SRP reinforce each other here."),
        ],
        DemoSourcePath: Src("Ocp/Export", "Demo.cs"),
        ContextNotes:
        [
            "With two formats and no third on the horizon, the if/else is fine (YAGNI). The refactor to plug-ins is mechanical, so do it when the third format arrives.",
            "Plug-in registries can be discovered by DI or reflection, but an explicit list in one place is easier to read and debug.",
        ]);
}
