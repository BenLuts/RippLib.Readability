# RippLib.Readability.EFExtensions
Async Entity Framework extension methods that wrap `AnyAsync()` with readable alternatives — so your EF Core queries read like intent, not mechanics.

To install use NuGet package manager or
```
dotnet add package RippLib.Readability.EFExtensions
```

> **Companion package:** `RippLib.Readability.EFExtensions` pairs with `RippLib.Readability`. Add both to get the full set of readability extensions across sync and async EF Core scenarios.

All extensions reside in the `RippLib.Readability.EFExtensions` namespace.
If you are using Global Usings simply add
```
RippLib.Readability.EFExtensions
```

## Current available methods:

```
.NotEmptyAsync()
```
Asynchronously determines whether a sequence has elements. Readable replacement for `await query.AnyAsync()`.
Instead of writing `if (await dbSet.AnyAsync(ct))` use `if (await dbSet.NotEmptyAsync(ct))`

```
.EmptyAsync()
```
Asynchronously determines whether a sequence has no elements. Readable replacement for `!await query.AnyAsync()`.
Instead of writing `if (!await dbSet.AnyAsync(ct))` use `if (await dbSet.EmptyAsync(ct))`

```
.ContainsAnyAsync(predicate)
```
Asynchronously determines whether any element satisfies a condition. Readable replacement for `await query.AnyAsync(predicate)`.
Instead of writing `if (await dbSet.AnyAsync(x => x.Active, ct))` use `if (await dbSet.ContainsAnyAsync(x => x.Active, ct))`

```
.ContainsNoneAsync(predicate)
```
Asynchronously determines whether no elements satisfy a condition. Readable replacement for `!await query.AnyAsync(predicate)`.
Instead of writing `if (!await dbSet.AnyAsync(x => x.Active, ct))` use `if (await dbSet.ContainsNoneAsync(x => x.Active, ct))`

## Roslyn Analyzers Included

This package ships Roslyn analyzers and code fixes. When `RippLib.Readability.EFExtensions` is referenced, the analyzers will suggest these methods automatically:

- `AnyAsync()` → `NotEmptyAsync()` (RLANY003)
- `!AnyAsync()` → `EmptyAsync()` (RLANY004)
- `AnyAsync(predicate)` → `ContainsAnyAsync(predicate)` (RLANY005)
- `!AnyAsync(predicate)` → `ContainsNoneAsync(predicate)` (RLANY006)

> The analyzers only fire when this package is referenced — projects that do not reference `RippLib.Readability.EFExtensions` see no suggestions.

[![Release to NuGet](https://github.com/BenLuts/RippLib.Readability/actions/workflows/release.yml/badge.svg)](https://github.com/BenLuts/RippLib.Readability/actions/workflows/release.yml)
