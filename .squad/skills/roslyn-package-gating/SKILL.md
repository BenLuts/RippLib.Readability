# SKILL: Roslyn Analyzer — Gating on Companion Package Presence

## Problem

A Roslyn analyzer ships alongside a library package (e.g. a NuGet package that ships both DLLs and analyzers). The analyzer suggests replacing standard API calls with methods from that same library. If a consumer references the analyzer assembly _without_ the library (e.g., via a separate base package), the analyzer fires warnings that point to unavailable types — false positives.

## Solution Pattern

Use `RegisterCompilationStartAction` to check whether the companion type is resolvable before registering any diagnostic actions.

```csharp
private const string CompanionTypeName = "My.Library.Namespace.WellKnownType";

public override void Initialize(AnalysisContext context)
{
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterCompilationStartAction(compilationContext =>
    {
        // Gate: only activate when the companion library is referenced.
        if (compilationContext.Compilation.GetTypeByMetadataName(CompanionTypeName) is null)
            return;

        compilationContext.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        // register any other actions here...
    });
}
```

## Choosing the Well-Known Type

Pick a **public, stable, non-generic type** from the companion assembly that:
- Is unlikely to be renamed.
- Is in the primary namespace of the library.
- Is the class that contains the extension/replacement methods the analyzer suggests.

Example for `RippLib.Readability.EFExtensions`:
```
"RippLib.Readability.EFExtensions.QueryableExtensions"
```

Avoid using internal types, generic types (their metadata name includes backtick syntax), or types from third-party dependencies.

## Why `GetTypeByMetadataName` over `ReferencedAssemblyNames`

| Approach | Pros | Cons |
|---|---|---|
| `GetTypeByMetadataName(typeName)` | Confirms the exact type exists; survives assembly renames; works with type forwarding | Slightly more expensive (one symbol lookup per compilation) |
| `compilation.ReferencedAssemblyNames.Any(n => n.Name == "AssemblyName")` | Cheap string check | Breaks on assembly renames; doesn't confirm types exist |

Prefer `GetTypeByMetadataName` — it's the Roslyn-idiomatic way.

## Testing

Always write two categories of tests for gated analyzers:

### 1. Fires when companion IS present
```csharp
await new ProjectBuilder()
    .AddEntityFramework()
    .AddRippLibReadabilityEFReference()   // companion package
    .WithAnalyzer<MyAnalyzer>()
    .WithSourceCode(codeWithDiagnostic)
    .ShouldReportDiagnostic()
    .ValidateAsync();
```

### 2. Silent when companion is ABSENT
```csharp
[Fact]
public async Task DoesNotTriggerWhenCompanionPackageAbsent()
{
    var test = /* same triggering code, no [| |] markers */;

    await new ProjectBuilder()
        .AddEntityFramework()
        // NOTE: no AddRippLibReadabilityEFReference()
        .WithAnalyzer<MyAnalyzer>()
        .WithSourceCode(test)
        .ValidateAsync();   // no ShouldReportDiagnostic()
}
```

## Applied In This Repo

Used to gate `RLANY003`–`RLANY006` (the four async EF Core analyzers) on the presence of `RippLib.Readability.EFExtensions.QueryableExtensions`.

See: `Analyzers/Analyzers/NotEmptyAsyncOverAnyAsync.cs` et al.
