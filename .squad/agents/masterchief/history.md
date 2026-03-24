# Project Context

- **Project:** RippLib.Readability
- **Created:** 2026-03-23

## Core Context

Agent Masterchief initialized as C# Specialist.

## Recent Updates

📌 Joined team on 2026-03-23 (renamed from Hockney)

## Learnings

Initial setup complete.

### 2026-03-25 — NuGet README Wiring

**Change:** Added `<PackageReadmeFile>` to both NuGet package projects and wired in the README files as pack items.

**Pattern used:**
- In `PropertyGroup`: `<PackageReadmeFile>README.md</PackageReadmeFile>`
- In the pack `ItemGroup`: `<None Include="path\to\README.md" Pack="true" PackagePath="" />`
- `PackagePath=""` places the file at the NuGet package root, which is required by NuGet for README display.

**Files changed:**
- `Package\Package.csproj` — references `<None Include="..\README.md" .../>` (root README.md, shared)
- `QueryableExtensions.Package\QueryableExtensions.Package.csproj` — references `<None Include="README.md" .../>` (package-local README)
- `QueryableExtensions.Package\README.md` — created, documents all four async methods and Roslyn analyzer gating

**Build result:** 0 errors, 0 warnings.

---

### 2026-03-25 — Version Bump for Analyzer Gating Fix

**Change:** Bumped both NuGet packages following the RLANY003–RLANY006 companion-package gating fix.

- `RippLib.Readability` (`Package\Package.csproj`): `0.0.0.11` → `0.0.0.12`
- `RippLib.Readability.EFExtensions` (`QueryableExtensions.Package\QueryableExtensions.Package.csproj`): `0.0.0.1` → `0.0.0.2`

**Why both?** Both package `.csproj` files include `Analyzers\Analyzers\bin\Release\netstandard2.0\Analyzers.dll` and `Analyzers.CodeFixes.dll` directly in their `<ItemGroup>`. The Roslyn.Analyzers fix ships inside that DLL, so every consumer of either package needs the corrected binary.

**Build result:** `dotnet build --configuration Release` — 0 errors, 0 warnings.

---

### 2026-03-25 — IEnumerable Analyzer Gating Fix

**Bug:** The two IEnumerable analyzers (`RLANY001` — `NotEmptyOverAny`, `RLANY002` — `EmptyOverNotAny`) were firing unconditionally on any `.Any()` call, even in projects that referenced only `RippLib.Readability.EFExtensions` without the main `RippLib.Readability` package. Since both packages ship the same `Analyzers.dll`, EFExtensions-only users saw IEnumerable diagnostics pointing to types that were not available in their compilation.

**Root cause:** Both analyzers registered `SyntaxNodeAction` directly in `Initialize()` with no check for whether the replacement types (`NotEmpty()`, `Empty()`) were reachable in the compilation.

**Fix — the gating pattern (same as RLANY003–RLANY006):**
```csharp
private const string MainPackageTypeName = "RippLib.Readability.EnumerableExtensions";

public override void Initialize(AnalysisContext context)
{
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(compilationContext =>
    {
        if (compilationContext.Compilation.GetTypeByMetadataName(MainPackageTypeName) is null)
            return;

        compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    });
}
```

**Presence check used:** `compilation.GetTypeByMetadataName("RippLib.Readability.EnumerableExtensions")` — the primary static extension class from the main `RippLib.Readability` assembly.

**Affected analyzers:** `NotEmptyOverAny`, `EmptyOverNotAny`.

**Tests updated:** Both test classes — all `Triggers*` / positive tests now include `AddRippLibReadabilityReference()`. Each class gained a `DoesNotTriggerWhenMainPackageAbsent` test. Result: 47 tests pass per TFM (net8/9/10) — up from 45.

**Structural difference vs IQueryable gate:** The IQueryable gate checked for a type in a *companion* package (`EFExtensions.QueryableExtensions`); this gate checks for a type in the *owning* package itself (`RippLib.Readability.EnumerableExtensions`). The mechanism is identical — only the direction of the dependency differs.

---

### 2026-03-25 — IQueryable Analyzer Gating Fix

**Bug:** The four async analyzers (RLANY003–RLANY006) were firing unconditionally on any `AnyAsync()` call from EF Core, even in projects that did not reference the companion `RippLib.Readability.EFExtensions` / `QueryableExtensions` package. There was nothing to suggest those methods as alternatives.

**Root cause:** All four analyzers registered their `SyntaxNodeAction` directly in `Initialize()`, with no check for whether the replacement types were available in the consuming project's compilation.

**Fix — the gating pattern:**
```csharp
private const string CompanionTypeName = "RippLib.Readability.EFExtensions.QueryableExtensions";

public override void Initialize(AnalysisContext context)
{
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(compilationContext =>
    {
        if (compilationContext.Compilation.GetTypeByMetadataName(CompanionTypeName) is null)
            return;

        compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    });
}
```

**Presence check used:** `compilation.GetTypeByMetadataName("RippLib.Readability.EFExtensions.QueryableExtensions")` — resolves the primary static extension class from the companion assembly. Returns `null` when the package is absent.

**Affected analyzers:** `NotEmptyAsyncOverAnyAsync`, `EmptyAsyncOverNotAnyAsync`, `ContainsAnyAsyncOverAnyAsync`, `ContainsNoneAsyncOverNotAnyAsync`.

**Tests updated:** All four test classes — `Triggers*` tests now include `AddRippLibReadabilityEFReference()` so the companion type is present in the test compilation. Each class gained a `DoesNotTriggerWhenCompanionPackageAbsent` test. Result: 45 tests pass (was 41) across net8/9/10.

### 2026-03-24 — .sln → .slnx Migration

Ran `dotnet sln migrate` to produce `RippLib.Readability.slnx`. Key gotchas:
- The migrate command does **not** delete the old `.sln` — must be removed manually afterwards.
- `RippLib.Readability.sln.DotSettings.user` was a Rider/ReSharper artefact tied to the old `.sln`; deleted alongside it.
- The `.slnx` format is clean XML (`<Solution>` root) with explicit `<Configurations>`, `<Folder>`, and `<Project>` nodes — far more readable than the legacy `.sln` guid soup.
- Build: 0 errors, 0 warnings both immediately after migration and after restoring packages from scratch.


### 2026-03-23 — Copilot PR Review Fixes

**ContainsNoneAsync logic bug:**
The original implementation used `AnyAsync(Negate(predicate))`, which negates the expression *tree* and returns `true` when *any* element does **not** match the predicate. The correct semantics for "contains none" is `!await AnyAsync(predicate)`, which returns `true` only when **zero** elements satisfy the predicate. The server-side `Negate` helper was removed as it is no longer needed.

**ReducedFrom normalization pattern for Roslyn analyzers:**
When inspecting `IMethodSymbol.Parameters` in a Roslyn analyzer, the parameter list differs by call style:
- **Reduced form** (`query.AnyAsync(ct)`): `Parameters` excludes the `this` (source) parameter.
- **Static form** (`EntityFrameworkQueryableExtensions.AnyAsync(query, ct)`): `Parameters` includes the source.
The canonical fix: `var baseMethod = methodSymbol.ReducedFrom ?? methodSymbol;` then inspect `baseMethod.Parameters`. This always yields the full static-form parameter list regardless of call style.

**`netstandard2.0` LINQ on `ImmutableArray`:**
The Analyzers project targets `netstandard2.0` with `DisableImplicitNamespaceImports=true`. LINQ extension methods (`.Any()`, `.Count()`) on `ImmutableArray<T>` require an explicit `using System.Linq;`. The index-from-end syntax `[^1]` (requires `System.Index`) is unavailable in `netstandard2.0`; use `[array.Length - 1]` instead.

**Namespace consistency in test projects:**
`DbBootstrapper` was in a different namespace to `TestFixture`, forcing a cross-namespace `using`. Moving both into `RippLib.Readability.EFExtensions.Tests.Bootstrapping` eliminates the coupling and keeps the bootstrapping layer coherent.

**EF Core migration hygiene:**
The `InitialMigration` had two silent bugs: `Description` was `nvarchar(20)` instead of `nvarchar(200)` (mismatched with `HasMaxLength(200)`), and the `Product` table was created without a `PrimaryKey` constraint despite `HasKey(x => x.Id)` in entity config. Migrations must precisely mirror entity configurations — mismatches cause runtime failures or schema drift.
