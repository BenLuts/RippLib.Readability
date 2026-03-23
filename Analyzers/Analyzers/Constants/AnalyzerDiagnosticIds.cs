namespace Analyzers.Constants;
public static class AnalyzerDiagnosticIds
{
    public const string PreferNotEmptyOverAny = "RLANY001";
    public const string PreferEmptyOverNotAny = "RLANY002";
    public const string PreferNotEmptyAsyncOverAnyAsync = "RLANY003";
    public const string PreferEmptyAsyncOverNotAnyAsync = "RLANY004";
    public const string PreferContainsAnyAsyncOverAnyAsync = "RLANY005";
    public const string PreferContainsNoneAsyncOverNotAnyAsync = "RLANY006";
}
