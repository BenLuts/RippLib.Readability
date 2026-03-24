# Squad Decisions

## Active Decisions

### 1. Split PR CI workflow into Windows + Linux jobs

**Date:** 2026-03-23  
**Author:** Forge (GitHub Actions Specialist)  
**Status:** Accepted  

**Context:** `RippLib.Readability.Tests` targets `net472`, `net480`, `net481` (.NET Framework, Windows-only). `QueryableExtensions.Tests` uses Testcontainers with Linux SQL Server Docker image. Single-runner workflow cannot satisfy both.

**Decision:** Split `Validate PR` workflow into parallel jobs:
- `build-and-test-windows` on `windows-latest` — all tests except `QueryableExtensions.Tests`
- `build-and-test-linux` on `ubuntu-latest` — `QueryableExtensions.Tests` only

Both run in parallel; both must pass. Windows job uses `--filter "FullyQualifiedName!~QueryableExtensions"`.

**Consequences:**
- Two green checks required instead of one
- Build time unchanged (parallelism offsets overhead)
- Pattern for future OS-specific test constraints established

---

### 2. Serialize Roslyn Analyzer Test Runs with `-m:1`

**Date:** 2026-03-23  
**Author:** Forge (GitHub Actions Specialist)  
**Status:** Accepted & Implemented  

**Context:** `Analyzers.Test` targets `net8.0`, `net9.0`, `net10.0`. `dotnet test` without constraints spawns parallel MSBuild workers. All three race to download/cache Microsoft.EntityFrameworkCore@9.0.14 to shared LocalAppData path, causing file-lock on 25 of 41 tests.

**Decision:** Add `-m:1` to `dotnet test` on Windows runners executing `Analyzers.Test`. Forces sequential TFM execution, eliminating race.

**Changes:**
- `.github/workflows/pr.yml` — `Test (all except QueryableExtensions.Tests)` step
- `.github/workflows/release.yml` — `test-windows` job `Test` step

**Trade-off:** Sequential execution is slower (modest wall-clock increase for 3 TFMs), acceptable for reliability gain.

**Alternatives Rejected:**
- Retry logic — treats symptom, wastes CI minutes
- Per-TFM jobs — workflow complexity
- Custom cache path — out of scope (not our code)

---

---

### 5. Gate IQueryable Analyzers on Companion Package Presence

**Date:** 2026-03-25  
**Author:** Masterchief (C# Specialist)  
**Status:** Implemented

**Context:** The `RippLib.Readability.EFExtensions` package ships both `QueryableExtensions.dll` (the EF Core extension methods) and `Analyzers.dll` (Roslyn analyzers suggesting their use). When the main `RippLib.Readability` package also ships the analyzer assembly (both reference the `Analyzers` project), projects that reference only `RippLib.Readability` without the EF extensions companion still see analyzer warnings pointing to methods that don't exist in their compilation.

**Problem:** The four async analyzers (`RLANY003`–`RLANY006`) registered their `SyntaxNodeAction` unconditionally in `Initialize()` with no check for companion type availability.

**Decision:** Gate each async analyzer on the presence of `RippLib.Readability.EFExtensions.QueryableExtensions` in the consuming compilation using `RegisterCompilationStartAction` + `GetTypeByMetadataName`.

**Implementation:**
```csharp
context.RegisterCompilationStartAction(compilationContext =>
{
    if (compilationContext.Compilation.GetTypeByMetadataName(
            "RippLib.Readability.EFExtensions.QueryableExtensions") is null)
        return;
    
    compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
});
```

**Consequences:**
- Correct behavior: Async analyzers only fire when replacement methods are guaranteed to exist
- Zero noise for non-EF projects
- All four test classes updated to include `AddRippLibReadabilityEFReference()` and new `DoesNotTriggerWhenCompanionPackageAbsent` tests
- 45/45 tests pass (net8.0, net9.0, net10.0)

**Skill documented:** `.squad/skills/roslyn-package-gating/SKILL.md` for reuse on future multi-delivery analyzer scenarios.

---

## Archived Decisions (Inbox → Merged)

### 3. Fixed All 15 Copilot PR Review Comments

**Date:** 2026-03-23  
**Author:** Masterchief (C# Specialist)  
**Status:** Implemented  

Addressed all 15 Copilot PR review comments on the `feature/ef-compatibility` branch:
- **Critical bugs:** ContainsNoneAsync logic error, TestingDbContext.ReaderExecuting bypass, InitialMigration schema mismatch
- **XML docs:** NotEmptyAsync, EmptyAsync, ContainsNoneAsync corrections
- **Analyzer robustness:** Added ReducedFrom normalization to 4 analyzers, netstandard2.0-safe parameter access
- **Code quality:** ProjectBuilder initializers, encoding artifact fix, namespace consistency, unused variable discards, ContainsNo→ContainsNone rename

Build succeeded with 0 errors, 0 warnings.

---

### 4. Removed Dead RoslynVersion Configuration

**Date:** 2026-03-24  
**Author:** Forge (GitHub Actions Specialist)  
**Status:** Implemented  

Removed 84 lines of ineffective RoslynVersion conditional logic from `Directory.Build.Targets` that claimed to control Roslyn versions but never specified `Version` or `VersionOverride`. Simplified to single PropertyGroup with active constants matching actual default configuration. No functional change—package versions still sourced from `Directory.Packages.props`.

**Rationale:** Code should do what it claims. Dead configuration knobs are worse than dead code.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
