# Project Context

- **Project:** RippLib.Readability
- **Created:** 2026-03-23

## Core Context

Agent Forge initialized as GitHub Actions Specialist.

## Recent Updates

📌 Joined team on 2026-03-23

## Learnings

Initial setup complete.

### 2026-03-23 — CI split for mixed OS test requirements

- A solution can have test projects with conflicting OS requirements (Windows for .NET Framework TFMs, Linux for Testcontainers Docker). A single-runner workflow cannot satisfy both.
- `dotnet test --filter "FullyQualifiedName!~<Namespace>"` is a clean way to exclude a specific test project without listing every other project explicitly.
- When splitting into two parallel jobs, no `needs:` dependency is required — GitHub requires all required jobs to pass before merging, so parallelism comes for free.
- VSIX projects (Analyzers.Vsix, Roslyn.Analyzers.Vsix) target net472 but are build-only; they do not add any OS constraint to the test step.

### 2026-03-23 — Roslyn analyzer test DLL lock race across TFMs

- `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` caches reference assemblies to a shared path (`AppData\Local\AnalyzerTests\ref\<package>@<version>\`). When `dotnet test` runs a multi-TFM project without `-m:1`, MSBuild spawns parallel worker nodes — one per TFM — and all race to write the same DLL, producing `System.IO.IOException: The process cannot access the file … because it is being used by another process.`
- Fix: pass `-m:1` to `dotnet test` to cap MSBuild at a single node, serializing TFM runs and eliminating the race. This is the standard CI workaround for this class of Roslyn analyzer test concurrency issues.
- Apply `-m:1` to every `dotnet test` invocation on a Windows runner that exercises a project with multiple TFMs (currently `Analyzers.Test`).

### 2026-03-24 — Misleading RoslynVersion build configuration

- `Directory.Build.Targets` had conditional blocks (`Choose`/`When`) that selected Roslyn package references based on `RoslynVersion` property, BUT the `PackageReference Update` statements never specified `Version` or `VersionOverride`. This meant the property had no effect on actual package versions (NuGet always used Directory.Packages.props defaults).
- The `ROSLYN_*` and `CSHARP*` compilation constants defined in each `When` block were misleading because they claimed to correspond to specific Roslyn versions, but the actual package versions were not controlled by the configuration switch.
- **Fix:** Removed all conditional logic and simplified to a single set of constants matching the default/current build configuration. If version pinning is needed in the future, the infrastructure would have to be restored with proper `VersionOverride` values in each conditional block.
- This is a build infrastructure cleanliness issue — dead code in build files can be even more dangerous than dead code in source, because it silently fails to do what it claims.
