# SOLID Showdown

An interactive Blazor app for learning the five SOLID principles, one at a time.

```bash
dotnet run          # then open http://localhost:5180
```

Requires the .NET 10 SDK. There are no NuGet dependencies.

## What each principle page has

1. **Structure & change impact.** Class diagrams of a design that *breaks* the principle and one that *follows* it.
   Pick a change request ("add a format", "the CFO changes overtime") and every box is coloured by what happens to it:
   modified, added, re-tested, or **broken** (changed behaviour nobody asked for).
2. **The code.** The real C# source, side by side. `❌` / `✅` comments mark the key lines, and `🤔 CONTEXT` comments
   discuss when the rule does or doesn't apply.
3. **Run it.** The same checks run against both designs. LSP runs contract tests against every subtype, and ISP calls
   every interface member through reflection.
4. **It depends.** When the "bad" version is actually fine.
5. **Quiz.** Three questions per principle, each with an explanation.

## Examples

| | Example 1 | Example 2 |
|---|---|---|
| **S** | Payroll: three actors, one `Employee` class | Invoices: tax, layout and delivery mixed |
| **O** | Customer discounts: `switch` vs. strategy | Report export: `if/else` vs. plug-in formats |
| **L** | Rectangle & Square | Bank accounts: pre/postconditions |
| **I** | Office devices: fat `IMachine` | Repositories: CRUD vs. reader/writer roles |
| **D** | Order notifications: `new SmtpEmailSender()` | Trial expiry: `DateTime.UtcNow` vs. `TimeProvider` |

## Layout

- `Examples/<Principle>/<Example>/` holds `Bad*.cs`, `Good*.cs` and `Demo.cs`. They are compiled (so demos really run)
  **and** embedded as resources (so the UI shows exactly the code that ran).
- `Catalog/Principles/*.cs` holds definitions, diagrams, change scenarios and quizzes.
- `Components/` holds the Blazor UI. `DiagramView.razor` renders the SVG diagrams.

## Adding an example

1. Add `Bad.cs`, `Good.cs` and `Demo.cs` (with `RunBad(DemoLog)` / `RunGood(DemoLog)`) under `Examples/...`.
   Use their own namespaces (`...Bad`, `...Good`) so the type names can match.
2. Add an `Example` to the principle's file in `Catalog/Principles/`: a diagram per side (nodes on a col/row grid)
   and some `ChangeScenario`s.
