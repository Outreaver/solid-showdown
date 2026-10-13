# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet run      # serves on http://localhost:5180 (see Properties/launchSettings.json)
dotnet build
```

.NET 10 SDK, no NuGet dependencies. There is no test project; the "tests" are the demos that run in the UI
(`Demo.RunBad` / `Demo.RunGood`), so verify behaviour changes by running the app and opening the principle page.

## Architecture

Blazor Web App with global Interactive Server rendering (`Components/App.razor`). Two routes: `Home.razor` (`/`)
and `PrinciplePage.razor` (`/principle/{Slug}`). All content is data in C#; the UI is generic over it.

**Data flow:** `SolidCatalog.All` → `PrincipleInfo` (one per file in `Catalog/Principles/`) → `Example`s, each with a
`Bad` and `Good` `Variant` (source paths, `Diagram`, and an `Action<DemoLog> Run`) plus `ChangeScenario`s.
Types live in `Catalog/Models.cs`.

**Example code is both compiled and displayed.** `SolidShowdown.csproj` embeds every `Examples/**/*.cs` as a resource
named `Examples/<path>`. `Catalog/SourceCode.cs` reads those resources and does its own regex-based syntax highlighting,
so what the UI shows is exactly what the demo ran. Consequences:
- Comments in example files are user-facing content. `❌`, `✅` and `CONTEXT` in a comment trigger special highlighting
  (`tk-bad` / `tk-good` / `tk-context`).
- Source paths in the catalog are built with `Dsl.Src("Ocp/Discounts", "Bad.cs")` and must match the file path exactly
  (a missing one throws `FileNotFoundException` at render).

**Example folder conventions** (`Examples/<Principle>/<Example>/`):
- `Bad.cs` / `Good.cs` use namespaces `...<Example>.Bad` / `...<Example>.Good` so type names can be identical.
- `Bad.AfterX.cs` / `Good.AfterX.cs` show the code after a change request; they use a further nested namespace
  (e.g. `...Discounts.Bad.AfterStudent`) and are listed in the variant's `SourcePaths`.
- `Demo.cs` exposes `static RunBad(DemoLog)` and `RunGood(DemoLog)`, aliasing the two namespaces as `Bad` / `Good`.

**Demos** write to `DemoLog` (`Catalog/DemoLog.cs`): lines (`Header`, `Ok`, `Fail`, `Warn`, `Muted`, `Check`) and tables.
`Catalog/ContractTests.cs` provides `log.RunContract(...)`, the rule × subtype matrix used for LSP.

**Diagrams and change impact.** Catalog files use the terse helpers in `Catalog/Dsl.cs` (`using static ...Dsl`):
`Cls`/`Ifc`/`Actor`/`Ext` nodes on a col/row grid (fractions allowed), `Uses`/`Impl`/`News`/`Asks` edges,
`WithBands(...)` for labelled regions. A `ChangeScenario` colours nodes per side via
`Impacts(summary, Mod(id), Add(id), Broke(id), Retest(id))` and can add nodes with `.Adding([...], edges)`.
Impact keys are node **ids** and must match ids in that variant's diagram. `Components/Shared/DiagramView.razor`
renders the SVG (grid constants `ColW`, `RowH`, `BoxW` there).

**Culture:** `Program.cs` forces `InvariantCulture` so demo output (decimals in CSV/JSON) is identical on every machine.

## Adding an example

1. Add `Bad.cs`, `Good.cs`, `Demo.cs` under `Examples/<Principle>/<Example>/` following the conventions above.
2. Add an `Example` to the principle's file in `Catalog/Principles/` and include it in that principle's `Examples` list:
   a diagram per side and some `ChangeScenario`s. Each principle also has `KeyIdeas`, `ItDepends` and a three-question `Quiz`.
