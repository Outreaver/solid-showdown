using static SolidShowdown.Catalog.Dsl;
using Clock_Demo = SolidShowdown.Examples.Dip.Clock.Demo;
using Notifications_Demo = SolidShowdown.Examples.Dip.Notifications.Demo;

namespace SolidShowdown.Catalog.Principles;

internal static class Dip
{
    public static PrincipleInfo Info => new(
        Slug: "dip",
        Letter: "D",
        Name: "Dependency Inversion Principle",
        OneLiner: "Depend on abstractions, not on volatile details.",
        Definition: "High-level modules should not depend on low-level modules. Both should depend on abstractions. Abstractions should not depend on details. Details should depend on abstractions.",
        DefinitionSource: "Robert C. Martin, The Dependency Inversion Principle (1996)",
        KeyIdeas:
        [
            "High-level policy (business rules) shouldn't know about low-level details (SMTP, SQL, the system clock, an SDK).",
            "The \"inversion\" is in the source-code dependency. Control still flows from policy to detail at runtime, but the detail now depends on an abstraction the policy owns.",
            "Ownership matters: the interface is phrased in the domain's language (IOrderNotifier.OrderPlaced), not the detail's (IEmailClient.SendSmtp).",
            "Concrete classes are wired together in one place, the composition root (Program.cs / DI container).",
        ],
        ItDepends:
        [
            "DIP targets volatile dependencies: I/O, network, time, randomness, third-party SDKs. Depending directly on string, List<T> or your own value objects is fine.",
            "An interface with one implementation that never changes and is never faked is just indirection. That's not DIP, it's ceremony.",
            "\"Abstraction\" doesn't have to mean interface. An abstract class (TimeProvider), a delegate (Func<DateTimeOffset>) or a plain method parameter can all invert a dependency.",
            "Dependency injection (DI) is a technique, and DIP is a direction. You can inject concrete classes (DI without DIP), or pass an abstraction by hand (DIP without a container).",
        ],
        Examples: [Notifications, Clock],
        Quiz:
        [
            new("What exactly is \"inverted\" in the Dependency Inversion Principle?", null,
                ["The direction of method calls at runtime.", "The direction of the source-code dependency: the low-level detail depends on an abstraction owned by the high-level policy.", "The order of constructor parameters.", "The inheritance hierarchy (children become parents)."],
                1,
                "At runtime OrderService still calls into the SMS code. In source code the SMS class references IOrderNotifier from the domain, so the arrow points up, against the flow of control."),
            new("Should OrderService depend on an IList<Order> abstraction instead of List<Order>, \"for DIP\"?", null,
                ["Yes, every concrete type should be hidden behind an interface.", "No. List<T> is stable and has no I/O; DIP is for volatile dependencies.", "Yes, otherwise it can't be tested.", "Only in libraries."],
                1,
                "DIP is about protecting policy from things that change or that you need to fake. List<T> is neither."),
            new("Is dependency injection the same as dependency inversion?", null,
                ["Yes, they are two names for one thing.", "No. DI is a way to supply dependencies; DIP is about which way dependencies point and who owns the abstraction.", "DI is the bad version of DIP.", "DIP requires a DI container."],
                1,
                "`new OrderService(new SmtpEmailSender())` is injection, but if the parameter type is the concrete SMTP class, the policy still depends on the detail."),
        ]);

    // The domain band deliberately excludes the right-hand columns: tests and the composition root
    // are the OUTERMOST layer - they know about everything, and nothing in the domain knows about them.
    private static readonly Band DomainBand = new("Domain · high-level policy", 0, 0, 0.5, 0.5);
    private static readonly Band InfraBand = new("Infrastructure · low-level details", 2.6, 3.9);

    private static Example Notifications => new(
        Id: "notifications",
        Title: "Order notifications: new SmtpEmailSender()",
        Summary: "A business service that creates its own SMTP sender. Then the same service depending on an IOrderNotifier that the domain owns, with SMTP, SMS and in-memory implementations plugged in from outside.",
        Bad: new(
            [Src("Dip/Notifications", "Bad.cs")],
            "Business rules create and call a concrete mail sender",
            "OrderService → SmtpEmailSender → SMTP server. You can't test the rules or change the channel without the mail infrastructure.",
            Diagram(
                [
                    Cls("OrderService", 1, 0, "business rules"),
                    Cls("SmtpEmailSender", 1, 2.6, "concrete, low-level"),
                    Ext("SMTP server", 1, 3.9, "smtp.company.local"),
                ],
                News("OrderService", "SmtpEmailSender"), Uses("SmtpEmailSender", "SMTP server"))
                .WithBands(DomainBand with { FromCol = 1, ToCol = 1 }, InfraBand),
            Notifications_Demo.RunBad),
        Good: new(
            [Src("Dip/Notifications", "Good.cs")],
            "The domain owns IOrderNotifier; details plug into it",
            "OrderService only knows IOrderNotifier, an interface in its own layer. Infrastructure classes point up at it, and the composition root picks one.",
            Diagram(
                [
                    Cls("OrderService", 0.5, 0, "business rules"), Ext("Composition root", 1.6, 0, "Program.cs / DI"),
                    Ifc("IOrderNotifier", 0.5, 1.3, "owned by the domain"),
                    Cls("SmtpOrderNotifier", 0, 2.6), Cls("SmsOrderNotifier", 1.2, 2.6), Cls("InMemoryOrderNotifier", 2.4, 2.6, "for tests"),
                    Ext("SMTP server", 0, 3.9),
                ],
                Uses("OrderService", "IOrderNotifier"),
                Impl("SmtpOrderNotifier", "IOrderNotifier"), Impl("SmsOrderNotifier", "IOrderNotifier"), Impl("InMemoryOrderNotifier", "IOrderNotifier"),
                Uses("SmtpOrderNotifier", "SMTP server"),
                News("Composition root", "OrderService", "wires"), News("Composition root", "SmsOrderNotifier", "picks"))
                .WithBands(DomainBand with { ToRow = 1.3 }, InfraBand),
            Notifications_Demo.RunGood),
        Scenarios:
        [
            new("Notify by SMS instead of e-mail", "Product",
                "Customers prefer text messages.",
                Impacts("OrderService is edited to create an SmsSender. The business rules are re-tested because of a delivery change.",
                    Mod("OrderService")).Adding([Cls("SmsSender", 2.3, 2.6)], News("OrderService", "SmsSender")),
                Impacts("SmsOrderNotifier plugs in, and the composition root changes one line. OrderService isn't opened.",
                    Mod("Composition root")),
                "Policy is protected from changes in mechanism."),
            new("Unit-test the order rules", "Developer",
                "A test for 'a valid order is accepted' that runs in milliseconds on CI.",
                Impacts("The test reaches the real SMTP sender and fails without a server (see the demo).",
                    Broke("SMTP server")).Adding([Ext("Unit tests", 2.3, 0)], Uses("Unit tests", "OrderService")),
                Impacts("The test passes an InMemoryOrderNotifier. No network, deterministic, fast.")
                    .Adding([Ext("Unit tests", 2.4, 1.3)], Uses("Unit tests", "OrderService"), News("Unit tests", "InMemoryOrderNotifier", "new")),
                "Testability is the everyday payoff of DIP, because the volatile thing can be replaced."),
            new("Mail library major upgrade", "IT Ops",
                "The SMTP library's API changes.",
                Impacts("SmtpEmailSender is rewritten. OrderService creates and calls that concrete class, so it is rebuilt and re-tested with it, and if Send's signature changes the business class is edited too.",
                    Mod("SmtpEmailSender"), Retest("OrderService")),
                Impacts("Only the adapter (SmtpOrderNotifier) changes. The domain never saw the library's API.",
                    Mod("SmtpOrderNotifier")),
                "Phrasing the abstraction in the domain's language keeps library churn out of your core."),
        ],
        DemoSourcePath: Src("Dip/Notifications", "Demo.cs"),
        ContextNotes:
        [
            "In a real solution the interface lives in the Domain project and the adapters in an Infrastructure project that references Domain, never the reverse. That's the Ports & Adapters / Clean Architecture shape.",
            "Don't create interfaces for pure, stable logic (IOrderTotalCalculator over arithmetic) just for DIP's sake.",
        ]);

    private static Example Clock => new(
        Id: "clock",
        Title: "Trial expiry: DateTimeOffset.UtcNow vs. TimeProvider",
        Summary: "A small, very common case. A business rule reads the system clock directly. The dependency is hidden, so tests become date-dependent. .NET's TimeProvider is a ready-made abstraction.",
        Bad: new(
            [Src("Dip/Clock", "Bad.cs")],
            "A hidden dependency on the system clock",
            "TrialService's results depend on when the code runs. Tests pass on the day they're written and fail later, with no code change.",
            Diagram(
                [
                    Cls("TrialService", 0.5, 0, "IsExpired · DaysLeft"), Ext("Tests", 1.9, 0),
                    Ext("DateTimeOffset.UtcNow", 0.5, 2.6, "global, static, uncontrollable"),
                ],
                Uses("TrialService", "DateTimeOffset.UtcNow", "hidden"), Uses("Tests", "TrialService"))
                .WithBands(DomainBand, InfraBand with { ToRow = 2.6 }),
            Clock_Demo.RunBad),
        Good: new(
            [Src("Dip/Clock", "Good.cs")],
            "Time is injected as an abstraction",
            "TrialService receives a TimeProvider. Production passes TimeProvider.System and tests pass a FixedClock they control.",
            Diagram(
                [
                    Cls("TrialService", 0.5, 0), Ext("Tests", 1.9, 0),
                    Ifc("TimeProvider", 0.5, 1.3, "owned by .NET, not the domain"),
                    Cls("TimeProvider.System", 0, 2.6, "real clock"), Cls("FixedClock", 1.2, 2.6, "test fake"),
                ],
                Uses("TrialService", "TimeProvider"), Uses("Tests", "TrialService"), News("Tests", "FixedClock"),
                Impl("TimeProvider.System", "TimeProvider"), Impl("FixedClock", "TimeProvider"))
                .WithBands(DomainBand, InfraBand with { ToRow = 2.6 }),
            Clock_Demo.RunGood),
        Scenarios:
        [
            new("Test \"expires one second after day 14\"", "Developer",
                "A boundary test for the trial rule.",
                Impacts("Impossible without waiting two weeks or editing TrialService. Existing date-based tests are already flaky.",
                    Broke("Tests")),
                Impacts("Set clock.Now and assert. Time travel is one line (see the demo)."),
                "Time, randomness and I/O are the classic hidden dependencies. Make them explicit."),
            new("Trial becomes 30 days", "Product",
                "A policy change.",
                Impacts("Edit TrialService.", Mod("TrialService")),
                Impacts("Edit TrialService.", Mod("TrialService")),
                "DIP doesn't help with policy changes and isn't meant to. The policy is the core that DIP protects from details."),
            new("Use a network-synchronised clock", "IT Ops",
                "Servers drift, so licence checks should use an NTP-backed time source.",
                Impacts("Every DateTimeOffset.UtcNow call in the codebase must be found and replaced.",
                    Mod("TrialService")),
                Impacts("A new TimeProvider subclass, wired in at the composition root.")
                    .Adding([Cls("NtpTimeProvider", 2.4, 2.6)], Impl("NtpTimeProvider", "TimeProvider")),
                "Once a detail is behind an abstraction, swapping it is an extension (DIP enables OCP)."),
        ],
        DemoSourcePath: Src("Dip/Clock", "Demo.cs"),
        ContextNotes:
        [
            "Sometimes the simplest inversion is a parameter: IsExpired(user, now). The caller decides what \"now\" is.",
            "TimeProvider is an abstract class, not an interface. DIP asks for an abstraction, not for the `interface` keyword.",
            "Strictly, this isn't full inversion: TimeProvider is owned by the .NET runtime, not by the domain. Depending on a stable abstraction the platform provides is a pragmatic shortcut. If you want the domain to own it, define an IClock in the domain and adapt TimeProvider to it.",
        ]);
}
