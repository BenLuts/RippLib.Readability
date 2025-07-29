; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID   | Category     | Severity     | Notes
RLANY001  | Readability  | Error        | Prefer .NotEmpty() over .Any() (NotEmptyOverAny)
RLANY002  | Readability  | Error        | Prefer .Empty() over !.Any() (EmptyOverNotAny)
