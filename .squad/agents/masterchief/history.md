# Project Context

- **Project:** RippLib.Readability
- **Created:** 2026-03-23

## Core Context

Agent Masterchief initialized as C# Specialist.

## Recent Updates

📌 Joined team on 2026-03-23 (renamed from Hockney)

## Learnings

Initial setup complete.

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
