# SOLID Showdown

An interactive Blazor app for learning the five SOLID principles, one at a time.

Every principle has two worked examples. Each example puts a design that **breaks** the principle next to one
that **follows** it. You can apply real change requests to both and see what each one has to touch, then run the
actual C#. The point is not "always do the right-hand version". Every page also shows when the left-hand version is fine.

## Run it

```bash
git clone https://github.com/Outreaver/solid-showdown.git
cd solid-showdown
dotnet run          # then open http://localhost:5180
```

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download). There are no NuGet dependencies.

## What each principle page has

1. **Structure & change impact.** Class diagrams of both designs. Pick a change request ("add a format",
   "the CFO changes overtime") and every box is coloured by what happens to it: modified, added, re-tested, or
   **broken** (changed behaviour nobody asked for).
2. **The code.** The real C# source, side by side. `❌` / `✅` comments mark the key lines, and `🤔 CONTEXT` comments
   discuss when the rule does or doesn't apply.
3. **Run it.** The same checks run against both designs. LSP runs contract tests against every subtype, and ISP calls
   every interface member through reflection to see which ones really work.
4. **It depends.** When the "bad" version is actually fine.
5. **Quiz.** Three questions per principle, each with an explanation.

## Examples

| | Example 1 | Example 2 |
|---|---|---|
| **S** | Payroll: three actors, one `Employee` class | Invoices: tax, layout and delivery mixed |
| **O** | Customer discounts: `switch` vs. strategy | Report export: `if/else` vs. plug-in formats |
| **L** | Rectangle & Square | Bank accounts: pre/postconditions |
| **I** | Office devices: fat `IMachine` | Repositories: CRUD vs. reader/writer roles |
| **D** | Order notifications: `new SmtpEmailSender()` | Trial expiry: `DateTimeOffset.UtcNow` vs. `TimeProvider` |

## Sources

The definitions follow the original wording:

| | Definition from |
|---|---|
| **S** | Robert C. Martin, *Clean Architecture* (2017): "responsible to one, and only one, actor" |
| **O** | Bertrand Meyer, *Object-Oriented Software Construction* (1988); polymorphic form by Robert C. Martin |
| **L** | Barbara Liskov & Jeannette Wing, *A Behavioral Notion of Subtyping* (1994) |
| **I** | Robert C. Martin, *The Interface Segregation Principle* (1996) |
| **D** | Robert C. Martin, *The Dependency Inversion Principle* (1996) |

## How it's built

- **The code you see is the code that ran.** `Examples/<Principle>/<Example>/` holds `Bad*.cs`, `Good*.cs` and
  `Demo.cs`. They are compiled (so the demos really run) **and** embedded as resources (so the UI shows them,
  with its own syntax highlighting).
- `Catalog/Principles/*.cs` holds the lesson content as data: definitions, diagrams, change scenarios and quizzes.
- `Components/` holds the Blazor UI (Interactive Server). `DiagramView.razor` renders the SVG diagrams.

## Contributing an example

1. Add `Bad.cs`, `Good.cs` and `Demo.cs` (with `RunBad(DemoLog)` / `RunGood(DemoLog)`) under
   `Examples/<Principle>/<Example>/`. Give them their own namespaces (`...Bad`, `...Good`) so the type names can match.
   Comments in these files are shown to readers, so write them for learners.
2. Add an `Example` to the principle's file in `Catalog/Principles/`: a diagram per side (nodes on a col/row grid)
   and some `ChangeScenario`s.
3. There is no test project. The demos are the tests: run the app, open the principle page and check that each
   demo's output says what the lesson claims.
