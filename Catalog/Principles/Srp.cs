using static SolidShowdown.Catalog.Dsl;
using Payroll_Demo = SolidShowdown.Examples.Srp.Payroll.Demo;
using Invoice_Demo = SolidShowdown.Examples.Srp.Invoice.Demo;

namespace SolidShowdown.Catalog.Principles;

internal static class Srp
{
    public static PrincipleInfo Info => new(
        Slug: "srp",
        Letter: "S",
        Name: "Single Responsibility Principle",
        OneLiner: "A module should have one, and only one, reason to change.",
        Definition: "A module should be responsible to one, and only one, actor.",
        DefinitionSource: "Robert C. Martin, Clean Architecture (2017) — refining his earlier \"one reason to change\"",
        KeyIdeas:
        [
            "\"Responsibility\" means a source of change requests, not \"does one thing\". Doing one thing is a rule for functions, not for SRP.",
            "An actor is a person or group who will ask for changes for the same reason, like Accounting, HR or the DBAs.",
            "Code that serves several actors gets edited by all of them. Then one team's change silently breaks another team's feature.",
            "The fix is to separate the code each actor depends on, and keep together what changes together (cohesion).",
        ],
        ItDepends:
        [
            "SRP is about people and organisations, so the same class can violate SRP at one company and be fine at another.",
            "In a small team where one person decides everything there is one actor. Splitting is then ceremony.",
            "Identical code is not necessarily duplicated knowledge. Two copies of the same formula owned by different actors is accidental duplication, and merging them couples those actors.",
            "Over-splitting hurts too. Cutting code that the same actor always changes together lowers cohesion and makes changes harder.",
        ],
        Examples: [Payroll, Invoice],
        Quiz:
        [
            new("A UserService has Register(), HashPassword() and SendWelcomeEmail(). Which question best tells you whether it violates SRP?",
                null,
                ["Does it have more than three methods?", "Do different people or teams ask for changes to these parts?", "Is the file longer than 200 lines?", "Does each method do exactly one thing?"],
                1,
                "SRP is about who asks for changes. If Security owns hashing, Product owns registration and Marketing owns the welcome e-mail, there are three actors and three reasons to change. Method count and file length are only symptoms."),
            new("Two classes contain the same three-line formula for \"regular hours\": one is used for payroll, one for the HR compliance report. Should you extract a shared helper?",
                "decimal regular = week.Sum(d => Math.Min(d.Hours, 8m));",
                ["Yes, DRY always wins.", "Not necessarily. If the rules belong to different actors, the duplication is accidental and merging it couples them.", "Yes, but make the helper virtual.", "No. Duplication is never a problem."],
                1,
                "DRY is about knowledge, not identical characters. Payroll's and HR's rules match today but are decided by different people. Merging them is how the CFO's change broke the COO's report in the first example."),
            new("Marketing always changes the header, body and footer of the invoice layout together. Is splitting InvoiceRenderer into HeaderRenderer, BodyRenderer and FooterRenderer \"better SRP\"?",
                null,
                ["Yes, smaller classes are always more SRP.", "No. It's one actor with one reason to change, so the split only lowers cohesion.", "Yes, ISP requires it.", "Only if each one gets an interface."],
                1,
                "SRP tells you where to cut (between actors), not to cut as small as possible. Things that change together for the same reason belong together."),
        ]);

    private static Example Payroll => new(
        Id: "payroll",
        Title: "Payroll: three actors, one class",
        Summary: "Robert C. Martin's classic case. Employee calculates pay (Accounting), reports hours (HR) and saves itself (DBAs). A shared helper turns an innocent change by one actor into a bug for another.",
        Bad: new(
            [Src("Srp/Payroll", "Bad.cs"), Src("Srp/Payroll", "Bad.AfterCfoChange.cs")],
            "One Employee class serves the CFO, the COO and the CTO",
            "Three actors edit one file. CalculatePay() and ReportHours() share the private RegularHours() helper, so when Accounting changes the overtime rule, HR's report changes too.",
            Diagram(
                [
                    Actor("CFO", 0, 0, "Accounting"), Actor("COO", 1, 0, "HR / Operations"), Actor("CTO", 2, 0, "DBAs"),
                    Cls("Employee", 1, 1.4, "CalculatePay · ReportHours · Save · RegularHours()"),
                ],
                Asks("CFO", "Employee"), Asks("COO", "Employee"), Asks("CTO", "Employee")),
            Payroll_Demo.RunBad),
        Good: new(
            [Src("Srp/Payroll", "Good.cs"), Src("Srp/Payroll", "Good.AfterCfoChange.cs")],
            "One class per actor; shared data has no owner",
            "PayCalculator, HourReporter and EmployeeRepository each answer to one actor. The \"regular hours\" formula is deliberately duplicated, because it is two rules owned by two people.",
            Diagram(
                [
                    Actor("CFO", 0, 0, "Accounting"), Actor("COO", 1, 0, "HR / Operations"), Actor("CTO", 2, 0, "DBAs"),
                    Cls("PayCalculator", 0, 1.4), Cls("HourReporter", 1, 1.4), Cls("EmployeeRepository", 2, 1.4),
                    Cls("EmployeeData", 1, 2.7, "plain data, no rules"),
                ],
                Asks("CFO", "PayCalculator"), Asks("COO", "HourReporter"), Asks("CTO", "EmployeeRepository"),
                Uses("PayCalculator", "EmployeeData"), Uses("HourReporter", "EmployeeData"), Uses("EmployeeRepository", "EmployeeData")),
            Payroll_Demo.RunGood),
        Scenarios:
        [
            new("Overtime starts after 7.5 h per day (for pay)", "CFO",
                "Accounting wants overtime pay to begin after 7.5 hours instead of 8.",
                Impacts("Employee.RegularHours() is edited. HR's compliance report silently changes, and the COO now gets wrong numbers without asking for anything.",
                    Mod("Employee"), Broke("COO")),
                Impacts("Only PayCalculator is edited. HourReporter is never opened, so it can't break.",
                    Mod("PayCalculator")),
                "The cost of an SRP violation isn't the size of the class. It's that a change for one actor lands in another actor's results. Run both demos to see it."),
            new("Move employees to a document database", "CTO",
                "The DBAs migrate from SQL to a document store.",
                Impacts("Persistence lives in the same class as pay and hour rules. The whole class is redeployed, and Accounting and HR have to re-verify their features.",
                    Mod("Employee"), Retest("CFO"), Retest("COO")),
                Impacts("Only EmployeeRepository changes.",
                    Mod("EmployeeRepository")),
                "Even when nothing breaks, mixing actors means more people have to re-test and coordinate every release."),
            new("Track each employee's department", "COO",
                "HR wants the hours report to show the department, and it must be stored.",
                Impacts("One class is edited, but it's the class everyone else depends on.",
                    Mod("Employee")),
                Impacts("The data, HR's reporter and the repository are all edited. Three files, each a small change owned by one team.",
                    Mod("EmployeeData"), Mod("HourReporter"), Mod("EmployeeRepository")),
                "An honest caveat: SRP doesn't make every change local. A change to the shape of shared data crosses boundaries. You get small, owned edits instead of one big shared one."),
        ],
        DemoSourcePath: Src("Srp/Payroll", "Demo.cs"),
        ContextNotes:
        [
            "If one founder decides pay, reports and the database, there is one actor. The single Employee class is fine there.",
            "The facade at the bottom of Good.cs shows you can keep a convenient \"one object\" API and still separate the responsibilities behind it.",
        ]);

    private static Example Invoice => new(
        Id: "invoice",
        Title: "Invoices: tax, layout and delivery in one place",
        Summary: "A more everyday case. One service calculates VAT, renders HTML and sends the e-mail. It looks like \"one feature\", but three different groups own those parts.",
        Bad: new(
            [Src("Srp/Invoice", "Bad.cs")],
            "InvoiceService answers to Finance, Marketing and IT Ops",
            "You can't run Finance's tax logic without also rendering Marketing's HTML and triggering IT's e-mail delivery, and every edit redeploys all three.",
            Diagram(
                [
                    Actor("Finance", 0, 0, "tax rules"), Actor("Marketing", 1, 0, "layout"), Actor("IT Ops", 2, 0, "delivery"),
                    Cls("InvoiceService", 1, 1.4, "VAT · HTML · e-mail"),
                    Ext("Outbox", 1, 2.7, "SMTP"),
                ],
                Asks("Finance", "InvoiceService"), Asks("Marketing", "InvoiceService"), Asks("IT Ops", "InvoiceService"),
                Uses("InvoiceService", "Outbox")),
            Invoice_Demo.RunBad),
        Good: new(
            [Src("Srp/Invoice", "Good.cs")],
            "Each part answers to one group; a thin workflow composes them",
            "TaxCalculator, InvoiceRenderer and InvoiceMailer each have one owner. InvoiceWorkflow owns the order of the steps, which is a reason to change of its own.",
            Diagram(
                [
                    Actor("Finance", 0, 0, "tax rules"), Actor("Marketing", 1, 0, "layout"), Actor("IT Ops", 2, 0, "delivery"),
                    Cls("TaxCalculator", 0, 1.4), Cls("InvoiceRenderer", 1, 1.4), Cls("InvoiceMailer", 2, 1.4),
                    Actor("Product", 0, 2.7, "the process"), Cls("InvoiceWorkflow", 1, 2.7, "calculate → render → send"),
                    Ext("Outbox", 2, 2.7, "SMTP"),
                ],
                Asks("Finance", "TaxCalculator"), Asks("Marketing", "InvoiceRenderer"), Asks("IT Ops", "InvoiceMailer"), Asks("Product", "InvoiceWorkflow"),
                Uses("InvoiceWorkflow", "TaxCalculator"), Uses("InvoiceWorkflow", "InvoiceRenderer"), Uses("InvoiceWorkflow", "InvoiceMailer"),
                Uses("InvoiceMailer", "Outbox")),
            Invoice_Demo.RunGood),
        Scenarios:
        [
            new("New brand: logo and layout", "Marketing",
                "Marketing rebrands. New header, colours and line layout.",
                Impacts("InvoiceService is edited, so the tax calculation and e-mail code are redeployed with it. Finance and IT should re-test.",
                    Mod("InvoiceService"), Retest("Finance"), Retest("IT Ops")),
                Impacts("Only InvoiceRenderer is edited.", Mod("InvoiceRenderer")),
                "Separating by actor means a layout change can't touch tax logic, so Finance has no reason to worry about it."),
            new("Reduced VAT rate for books", "Finance",
                "Legal says books are taxed at 5%.",
                Impacts("Same class again. Marketing's and IT's code ships with Finance's change.",
                    Mod("InvoiceService"), Retest("Marketing"), Retest("IT Ops")),
                Impacts("Only TaxCalculator is edited, and its tests run in isolation (see the demo).", Mod("TaxCalculator")),
                "Isolated ownership also gives you isolated tests. Finance's test no longer sends e-mails."),
            new("Also archive a PDF copy", "Product",
                "Every invoice must also be stored as a PDF.",
                Impacts("InvoiceService grows a fourth responsibility.", Mod("InvoiceService")),
                Impacts("The workflow changes, because the process changed. A new PdfArchiver is added; nothing else is touched.",
                    Mod("InvoiceWorkflow")).Adding([Cls("PdfArchiver", 3, 1.4)], Uses("InvoiceWorkflow", "PdfArchiver")),
                "Orchestration is a responsibility too. Here it belongs to whoever owns the process."),
        ],
        DemoSourcePath: Src("Srp/Invoice", "Demo.cs"),
        ContextNotes:
        [
            "Don't keep slicing. HeaderRenderer, BodyRenderer and FooterRenderer would all be changed by Marketing together. That's the same actor, so it belongs in one class.",
            "If the same developer owns tax, layout and mail and they always change together, the original class is acceptable. Revisit it when ownership splits.",
        ]);
}
