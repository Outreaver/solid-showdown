using static SolidShowdown.Catalog.Dsl;
using Printers_Demo = SolidShowdown.Examples.Isp.Printers.Demo;
using Repository_Demo = SolidShowdown.Examples.Isp.Repository.Demo;

namespace SolidShowdown.Catalog.Principles;

internal static class Isp
{
    public static PrincipleInfo Info => new(
        Slug: "isp",
        Letter: "I",
        Name: "Interface Segregation Principle",
        OneLiner: "Don't make clients depend on methods they don't use.",
        Definition: "Clients should not be forced to depend upon interfaces that they do not use.",
        DefinitionSource: "Robert C. Martin, The Interface Segregation Principle (1996), from his consulting work at Xerox",
        KeyIdeas:
        [
            "\"Fat\" interfaces couple unrelated clients: a change made for one client forces recompiling, retesting and redeploying the others.",
            "Implementers get forced into stubs that throw NotSupportedException. That's a lie told at compile time and discovered at runtime, and an LSP violation too.",
            "Prefer role interfaces, shaped by what a particular client needs, over header interfaces that list everything a class can do.",
            "One class can implement many small interfaces, and interfaces compose (IOfficeDevice : IPrinter, IScanner…).",
        ],
        ItDepends:
        [
            "The clients decide where to cut, not the implementer. If every client that scans also prints and faxes, one combined interface is fine.",
            "One method per interface everywhere satisfies the letter of ISP and makes code hard to navigate: more types, more DI registrations, harder discovery.",
            "In dynamically typed or single-deployment codebases the recompile/redeploy argument is weaker. The design argument (least privilege, honest types, simpler test doubles) still holds.",
            "A plain CRUD screen that really does use all of Get/List/Add/Update/Delete is well served by IRepository<T>.",
        ],
        Examples: [Printers, Repository],
        Quiz:
        [
            new("What does ISP actually say?", null,
                ["Every interface should have exactly one method.", "No client should be forced to depend on methods it doesn't use.", "A class may implement only one interface.", "Interfaces must live in a separate assembly."],
                1,
                "The unit of measure is the client. Small interfaces are a common outcome, not the rule itself."),
            new("Who should decide how an interface is split?", null,
                ["The class that implements it.", "The clients that consume it.", "The DI container.", "Whoever writes the database schema."],
                1,
                "Role interfaces are defined by usage. That's why the same OrderRepository class can expose IReader<Order> to reporting and IWriter<Order> to checkout."),
            new("Which principles does this hurt?",
                "class GuestUser : IUser\n{\n    public void ChangePassword(string p) => throw new NotSupportedException();\n    // ...\n}",
                ["Only SRP.", "ISP (IUser is too fat for guests) and LSP (the stub breaks IUser's contract).", "Only DIP.", "None. NotSupportedException exists for this."],
                1,
                "ISP and LSP often show up together. A forced stub is the ISP problem, and the surprise exception for callers is the LSP problem."),
        ]);

    private static Example Printers => new(
        Id: "printers",
        Title: "Office devices: one fat interface",
        Summary: "An IMachine interface with Print, Scan, Fax and Staple. A simple inkjet has to \"implement\" three things it can't do. The demo calls every method through reflection to expose the lies.",
        Bad: new(
            [Src("Isp/Printers", "Bad.cs")],
            "Every device must claim every capability",
            "BasicInkjet throws for three of four methods, and PrintJobService only prints but is coupled to all four.",
            Diagram(
                [
                    Cls("PrintJobService", 1.2, 0, "uses Print() only"),
                    Ifc("IMachine", 1.2, 1.3, "Print · Scan · Fax · Staple"),
                    Cls("MultiFunctionPrinter", 0.5, 2.6), Cls("BasicInkjet", 1.9, 2.6, "3 of 4 methods throw"),
                ],
                Uses("PrintJobService", "IMachine"), Impl("MultiFunctionPrinter", "IMachine"), Impl("BasicInkjet", "IMachine")),
            Printers_Demo.RunBad),
        Good: new(
            [Src("Isp/Printers", "Good.cs")],
            "Role interfaces, composed where a client needs the bundle",
            "Each client depends on the role it uses. Devices claim only what they can do, and nothing throws NotSupportedException.",
            Diagram(
                [
                    Cls("PrintJobService", 0, 0),
                    Ifc("IPrinter", 0, 1.3), Ifc("IScanner", 1, 1.3), Ifc("IFax", 2, 1.3), Ifc("IStapler", 3, 1.3),
                    Cls("BasicInkjet", 0, 2.6), Ifc("IOfficeDevice", 1.5, 2.6, "composes all four"),
                    Cls("MultiFunctionPrinter", 1.5, 3.9),
                ],
                Uses("PrintJobService", "IPrinter"), Impl("BasicInkjet", "IPrinter"),
                Impl("IOfficeDevice", "IPrinter"), Impl("IOfficeDevice", "IScanner"), Impl("IOfficeDevice", "IFax"), Impl("IOfficeDevice", "IStapler"),
                Impl("MultiFunctionPrinter", "IOfficeDevice")),
            Printers_Demo.RunGood),
        Scenarios:
        [
            new("Fax gets a resolution parameter", "Hardware team",
                "Fax(document) becomes Fax(document, dpi).",
                Impacts("IMachine changes, so the inkjet that can't fax must be edited, and the print-only service is recompiled and retested.",
                    Mod("IMachine"), Mod("MultiFunctionPrinter"), Mod("BasicInkjet"), Retest("PrintJobService")),
                Impacts("Only IFax and the one device that actually faxes change.",
                    Mod("IFax"), Mod("MultiFunctionPrinter")),
                "Fat interfaces carry changes to clients and implementers that have nothing to do with them."),
            new("Add a cheap laser printer", "Product",
                "A print-only model.",
                Impacts("The new class needs three more throwing stubs.")
                    .Adding([Cls("CheapLaser", 3.0, 2.6, "3 throwing stubs")], Impl("CheapLaser", "IMachine")),
                Impacts("The new class implements IPrinter and nothing else.")
                    .Adding([Cls("CheapLaser", -1, 2.6)], Impl("CheapLaser", "IPrinter")),
                "Honest types: the interface a class implements should describe what it can really do."),
            new("New feature: scan to e-mail", "Product",
                "A service that scans a page and mails it.",
                Impacts("ScanToEmail asks for IMachine, so it happily accepts a BasicInkjet and crashes at runtime.",
                    Broke("BasicInkjet")).Adding([Cls("ScanToEmail", 2.6, 0)], Uses("ScanToEmail", "IMachine")),
                Impacts("ScanToEmail asks for IScanner. Passing an inkjet is a compile error.")
                    .Adding([Cls("ScanToEmail", 1, 0)], Uses("ScanToEmail", "IScanner")),
                "Narrow dependencies let the compiler check capability, not just type."),
        ],
        DemoSourcePath: Src("Isp/Printers", "Demo.cs"),
        ContextNotes:
        [
            "If your system only has one kind of client that needs everything, IOfficeDevice alone would be fine. The split pays for itself because different clients use different parts.",
            "Notice the overlap with LSP: the throwing stubs in BasicInkjet violate both.",
        ]);

    private static Example Repository => new(
        Id: "repository",
        Title: "Repositories: one CRUD interface for everyone",
        Summary: "A generic IRepository<T> used by a read-only report and implemented by an append-only audit log. The split into reader, appender and writer roles is Command-Query Separation applied to interfaces, the same idea CQRS takes further.",
        Bad: new(
            [Src("Isp/Repository", "Bad.cs")],
            "Read-only clients and append-only stores share a CRUD interface",
            "MonthlyReport can call Delete, and its test double needs five members. AuditLogStore must \"implement\" Update and Delete by throwing.",
            Diagram(
                [
                    Cls("MonthlyReport", 0.5, 0, "uses List() only"),
                    Ifc("IRepository<T>", 1.2, 1.3, "Get · List · Add · Update · Delete"),
                    Cls("OrderRepository", 0.5, 2.6), Cls("AuditLogStore", 1.9, 2.6, "Update/Delete throw"),
                ],
                Uses("MonthlyReport", "IRepository<T>"), Impl("OrderRepository", "IRepository<T>"), Impl("AuditLogStore", "IRepository<T>")),
            Repository_Demo.RunBad),
        Good: new(
            [Src("Isp/Repository", "Good.cs")],
            "Reader / appender / writer roles",
            "MonthlyReport gets IReader<Order> and nothing more. The audit log is append-only by type, not by runtime exception.",
            Diagram(
                [
                    Cls("MonthlyReport", 0, 0),
                    Ifc("IReader<T>", 0, 1.3, "Get · List"), Ifc("IWriter<T>", 1.2, 1.3, "Update · Delete"), Ifc("IAppender<T>", 2.4, 1.3, "Add"),
                    Cls("OrderRepository", 0.6, 2.6), Cls("AuditLogStore", 2.4, 2.6, "append-only by type"),
                ],
                Uses("MonthlyReport", "IReader<T>"), Impl("IWriter<T>", "IAppender<T>"),
                Impl("OrderRepository", "IReader<T>"), Impl("OrderRepository", "IWriter<T>"),
                Impl("AuditLogStore", "IReader<T>"), Impl("AuditLogStore", "IAppender<T>")),
            Repository_Demo.RunGood),
        Scenarios:
        [
            new("Write a unit test for MonthlyReport", "Developer",
                "It needs a fake order source.",
                Impacts("The fake must implement 5 members. The report only calls List; Get at least belongs to reading, but Add/Update/Delete are write-side noise that has to throw or do nothing.")
                    .Adding([Cls("FakeOrderRepository", 1.9, 0, "5 members, 3 write-side")], Impl("FakeOrderRepository", "IRepository<T>")),
                Impacts("The fake implements Get and List, and that's all.")
                    .Adding([Cls("FakeOrderReader", 1.2, 0, "2 members")], Impl("FakeOrderReader", "IReader<T>")),
                "Test-double pain is one of the fastest ways to feel an ISP violation."),
            new("Add BulkImport for the admin tool", "Admin team",
                "One new client needs to import 10k orders at once.",
                Impacts("IRepository grows a sixth member. Every implementation changes, including another throwing stub in the audit log, and every client is retested.",
                    Mod("IRepository<T>"), Mod("OrderRepository"), Mod("AuditLogStore"), Retest("MonthlyReport")),
                Impacts("A new role interface, implemented only where it makes sense.",
                    Mod("OrderRepository")).Adding([Ifc("IBulkImporter<T>", 3.5, 1.3)], Impl("OrderRepository", "IBulkImporter<T>")),
                "New client needs become new roles, and existing clients don't notice."),
            new("Someone calls Delete from the report", "Any developer",
                "A \"quick fix\" deletes test orders from inside MonthlyReport.",
                Impacts("It compiles. The reporting module can now destroy data.", Broke("MonthlyReport")),
                Impacts("Compile error: IReader<Order> has no Delete."),
                "Narrow interfaces are least privilege expressed in the type system."),
        ],
        DemoSourcePath: Src("Isp/Repository", "Demo.cs"),
        ContextNotes:
        [
            "Don't go to one interface per method (IGetter, ILister, IAdder…). Group by what clients actually use together.",
            "One class (OrderRepository) can still implement several roles. ISP is about what clients see, not about how many classes you have.",
        ]);
}
