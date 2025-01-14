using System.Globalization;

namespace Analyzers;

internal static class RuleIdentifiers
{
    public const string UseEmptyOverAny = "RL0001";
    

    public static string GetHelpUri(string identifier)
    {
        return string.Format(CultureInfo.InvariantCulture, "https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/Rules/{0}.md", identifier);
    }
}
