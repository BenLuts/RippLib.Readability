; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
RLANY001 | Usage | Warning | Prefer .NotEmpty() over .Any() (NotEmptyOverAny)
RLANY002 | Usage | Warning | Prefer .Empty() over !.Any() (EmptyOverNotAny)
RLANY003 | Usage | Warning | Prefer .NotEmptyAsync() over .AnyAsync() (NotEmptyAsyncOverAnyAsync)
RLANY004 | Usage | Warning | Prefer .EmptyAsync() over !.AnyAsync() (EmptyAsyncOverNotAnyAsync)
RLANY005 | Usage | Warning | Prefer .ContainsAnyAsync() over .AnyAsync(predicate) (ContainsAnyAsyncOverAnyAsync)
RLANY006 | Usage | Warning | Prefer .ContainsNoneAsync() over !.AnyAsync(predicate) (ContainsNoneAsyncOverNotAnyAsync)
