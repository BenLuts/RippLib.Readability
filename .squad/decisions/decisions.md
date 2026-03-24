# Team Decisions

## Decision: Migrate solution file to .slnx format

**Date:** 2026-03-24  
**Author:** Masterchief (C# Specialist)  
**Status:** Implemented

### Context

The solution used the legacy `.sln` format (GUID-heavy, hard to diff, difficult to read).
`dotnet sln migrate` became stable in .NET SDK 9 and produces an XML-based `.slnx` alternative.

### Decision

Migrate `RippLib.Readability.sln` → `RippLib.Readability.slnx` and delete the old file.
Also removed `RippLib.Readability.sln.DotSettings.user` (Rider artefact bound to the old file).

### Steps Taken

1. `dotnet sln RippLib.Readability.sln migrate` — generated `.slnx` alongside `.sln`.
2. `dotnet build RippLib.Readability.slnx` — 0 errors, 0 warnings.
3. Deleted `RippLib.Readability.sln` and `RippLib.Readability.sln.DotSettings.user`.
4. Second `dotnet build` — still 0 errors, 0 warnings.

### Consequences

- Solution file is now human-readable XML; diffs are meaningful.
- Requires .NET SDK 9+ tooling (already in use).
- `.DotSettings.user` was personal/local state — no functional loss.

---

## Decision: Explicit SDK Pin Required for .slnx Support in pr.yml

**Date:** 2026-03-24
**Author:** Forge (GitHub Actions Specialist)
**Status:** Implemented

### Context

The solution was migrated from `RippLib.Readability.sln` to `RippLib.Readability.slnx`. The `.slnx` format is only recognized by the .NET 9+ SDK when `dotnet restore/build/test` auto-discovers the solution file from the working directory.

`pr.yml` had no `actions/setup-dotnet` step, relying entirely on the pre-installed SDK on GitHub-hosted runners. This worked with `.sln` (supported since SDK 6), but `.slnx` requires a minimum of SDK 9.0 — creating a latent fragility.

### Decision

Add `actions/setup-dotnet@v4` with `dotnet-version: 8.0.x / 9.0.x / 10.0.x` to both jobs in `pr.yml`, matching the SDK matrix already used in `release.yml`.

### Consequences

- CI is now explicitly pinned and not dependent on runner image SDK defaults.
- Both `pr.yml` and `release.yml` use the same SDK matrix — consistent and maintainable.
- `10.0.x` is the active (last-listed) SDK in both workflows, guaranteeing `.slnx` compatibility.

### Files Changed

- `.github/workflows/pr.yml` — added `Setup .NET SDK` step to `build-and-test-windows` and `build-and-test-linux` jobs

### Files Audited and Confirmed Clean

- `.github/workflows/release.yml` — already had `setup-dotnet` with SDK 10.x
- `.github/workflows/squad-heartbeat.yml` — no dotnet commands
- `.github/workflows/squad-triage.yml` — no dotnet commands
- `.github/workflows/squad-issue-assign.yml` — no dotnet commands
- `.github/workflows/sync-squad-labels.yml` — no dotnet commands
