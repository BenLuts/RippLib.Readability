using Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestHelper;

public sealed partial class ProjectBuilder
{
    private static readonly ConcurrentDictionary<string, Lazy<Task<string[]>>> _nuGetPackagesCache = new(StringComparer.Ordinal);
    
    public OutputKind OutputKind { get; private set; } = OutputKind.DynamicallyLinkedLibrary;
    public string FileName { get; private set; }
    public string SourceCode { get; private set; } = "";
    public Dictionary<string, string> AnalyzerConfiguration { get; } = new();
    public Dictionary<string, string> AdditionalFiles { get; } = new();
    public bool IsValidCode { get; private set; } = true;
    public bool IsValidFixCode { get; private set; } = true;
    public LanguageVersion LanguageVersion { get; private set; } = LanguageVersion.Latest;
    public TargetFramework TargetFramework { get; private set; } = TargetFramework.NetLatest;
    public IList<MetadataReference> References { get; } = [];
    public IList<string> ApiReferences { get; } = [];
    public IList<DiagnosticAnalyzer> DiagnosticAnalyzer { get; } = [];
    public CodeFixProvider CodeFixProvider { get; private set; }
    public IList<DiagnosticResult> ExpectedDiagnosticResults { get; } = [];
    public string ExpectedFixedCode { get; private set; }
    public int? CodeFixIndex { get; private set; }
    public string DefaultAnalyzerId { get; set; }
    public string DefaultAnalyzerMessage { get; set; }

    private static Task<string[]> GetNuGetReferences(string packageName, string version, params string[] paths)
    {
        var task = _nuGetPackagesCache.GetOrAdd(
            packageName + '@' + version + ':' + string.Join(",", paths),
            _ => new Lazy<Task<string[]>>(Download));

        return task.Value;

        async Task<string[]> Download()
        {
            var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AnalyzerTests", "ref", packageName + '@' + version);
            if (!Directory.Exists(tempFolder) || !Directory.EnumerateFileSystemEntries(tempFolder).Any())
            {
                Directory.CreateDirectory(tempFolder);
                using var stream = await SharedHttpClient.Instance.GetStreamAsync(new Uri($"https://www.nuget.org/api/v2/package/{packageName}/{version}")).ConfigureAwait(false);
                using var zip = new ZipArchive(stream, ZipArchiveMode.Read);

                foreach (var entry in zip.Entries.Where(file => paths.Any(path => file.FullName.StartsWith(path, StringComparison.Ordinal))))
                {
                    entry.ExtractToFile(Path.Combine(tempFolder, entry.Name), overwrite: true);
                }
            }

            var dlls = Directory.GetFiles(tempFolder, "*.dll");

            // Filter invalid .NET assembly
            var result = new List<string>();
            foreach (var dll in dlls)
            {
                if (Path.GetFileName(dll) == "System.EnterpriseServices.Wrapper.dll")
                    continue;

                using var stream = File.OpenRead(dll);
                using var peFile = new PEReader(stream);
                result.Add(dll);
            }

            Assert.NotEmpty(result);
            return [.. result];
        }
    }

    public ProjectBuilder AddNuGetReference(string packageName, string version, string pathPrefix)
    {
        foreach (var reference in GetNuGetReferences(packageName, version, pathPrefix).Result)
        {
            References.Add(MetadataReference.CreateFromFile(reference));
        }

        return this;
    }

    public ProjectBuilder AddEntityFramework()
    {
        var (version, path) = Environment.Version.Major >= 10
            ? ("10.0.5", "lib/net10.0/")
            : Environment.Version.Major >= 9
                ? ("9.0.14", "lib/net9.0/")
                : ("9.0.14", "lib/net8.0/");

        return AddNuGetReference("Microsoft.EntityFrameworkCore", version, path);
    }

    public ProjectBuilder AddRippLibReadabilityReference()
    {
        // Find the output path for the Extensions project (RippLib.Readability)
        // and add it as a MetadataReference if not already present.
        var baseDir = AppContext.BaseDirectory;
        var dllName = "Extensions.dll";
        var possiblePaths = new[]
        {
            Path.Combine(baseDir, dllName),
            Path.Combine(baseDir, "..","..", "..", "..", "..", "RippLib.Readability", "bin", "Debug", "net8.0", dllName),
            Path.Combine(baseDir, "..","..", "..", "..", "..", "RippLib.Readability", "bin", "Debug", "net9.0", dllName),
            Path.Combine(baseDir, "..","..", "..", "..", "..", "RippLib.Readability", "bin", "Debug", "net10.0", dllName),
            Path.Combine(baseDir, "..","..", "..", "..", "..", "RippLib.Readability", "bin", "Debug", "netstandard2.0", dllName),
            Path.Combine(baseDir, "..","..", "..", "..", "..", "RippLib.Readability", "bin", "Debug", "netstandard2.1", dllName),
        };
        foreach (var path in possiblePaths)
        {
            if (File.Exists(path) && !References.Any(r => r.Display?.EndsWith(dllName) == true))
            {
                References.Add(MetadataReference.CreateFromFile(Path.GetFullPath(path)));
                break;
            }
        }
        return this;
    }

    public ProjectBuilder AddRippLibReadabilityEFReference()
    {
        // Find the output path for the Extensions project (RippLib.Readability)
        // and add it as a MetadataReference if not already present.
        var baseDir = AppContext.BaseDirectory;
        var dllName = "QueryableExtensions.dll";
        var possiblePaths = new[]
        {
            Path.Combine(baseDir, dllName),
            Path.Combine(baseDir, "..","..", "..", "..", "..", "QueryableExtensions", "bin", "Debug", "net8.0", dllName),
            Path.Combine(baseDir, "..","..", "..", "..", "..", "QueryableExtensions", "bin", "Debug", "net9.0", dllName),
            Path.Combine(baseDir, "..","..", "..", "..", "..", "QueryableExtensions", "bin", "Debug", "net10.0", dllName),
        };
        foreach (var path in possiblePaths)
        {
            if (File.Exists(path) && !References.Any(r => r.Display?.EndsWith(dllName) == true))
            {
                References.Add(MetadataReference.CreateFromFile(Path.GetFullPath(path)));
                break;
            }
        }
        return this;
    }

    public ProjectBuilder WithSourceCode([StringSyntax("C#-test")] string sourceCode) =>
        WithSourceCode(fileName: null, sourceCode);

    public ProjectBuilder WithSourceCode(string fileName, [StringSyntax("C#-test")] string sourceCode)
    {
        FileName = fileName;
        ParseSourceCode(sourceCode);
        return this;
    }

    /// <summary>
    /// <list type="bullet">
    ///   <item>[|code|]</item>
    ///   <item>{|ruleId:code|}</item>
    /// </list>
    /// </summary>
    /// <param name="sourceCode"></param>
    private void ParseSourceCode(string sourceCode)
    {
        var sb = new StringBuilder();
        var lineStart = -1;
        var columnStart = -1;

        var lineIndex = 1;
        var columnIndex = 1;
        char endChar = default;
        string ruleId = default;
        for (var i = 0; i < sourceCode.Length; i++)
        {
            var c = sourceCode[i];
            switch (c)
            {
                case '\n':
                    sb.Append(c);
                    lineIndex++;
                    columnIndex = 1;
                    break;

                case '{' when lineStart < 0 && Next() == '|':
                    lineStart = lineIndex;
                    columnStart = columnIndex;
                    endChar = '}';
                    i += 2;
                    ruleId = TakeUntil(':');
                    i += ruleId.Length;
                    break;

                case '[' when lineStart < 0 && Next() == '|':
                    lineStart = lineIndex;
                    columnStart = columnIndex;
                    i++;
                    endChar = ']';
                    break;

                case '|' when lineStart >= 0 && Next() == endChar:
                    ShouldReportDiagnostic(new DiagnosticResult
                    {
                        Id = ruleId ?? DefaultAnalyzerId,
                        Message = DefaultAnalyzerMessage,
                        Locations = [new DiagnosticResultLocation(FileName ?? "Test0.cs", lineStart, columnStart, lineIndex, columnIndex)],
                    });

                    lineStart = -1;
                    columnStart = -1;
                    endChar = default;
                    ruleId = default;
                    i++;
                    break;

                default:
                    sb.Append(c);
                    columnIndex++;
                    break;
            }

            char Next() => i + 1 < sourceCode.Length ? sourceCode[i + 1] : default;
            string TakeUntil(char c)
            {
                var span = sourceCode.AsSpan(i);
                var index = span.IndexOf(c);
                if (index < 0)
                    return span.ToString();

                return span[0..index].ToString();
            }
        }

        SourceCode = sb.ToString();
    }

    public ProjectBuilder WithAnalyzer(DiagnosticAnalyzer diagnosticAnalyzer, string id = null, string message = null)
    {
        DiagnosticAnalyzer.Add(diagnosticAnalyzer);
        DefaultAnalyzerId = id;
        DefaultAnalyzerMessage = message;
        return this;
    }

    public ProjectBuilder WithAnalyzer<T>(string id = null, string message = null) where T : DiagnosticAnalyzer, new() =>
        WithAnalyzer(new T(), id, message);

    public ProjectBuilder WithCodeFixProvider(CodeFixProvider codeFixProvider)
    {
        CodeFixProvider = codeFixProvider;
        return this;
    }

    public ProjectBuilder WithCodeFixProvider<T>() where T : CodeFixProvider, new() =>
        WithCodeFixProvider(new T());

    public ProjectBuilder ShouldReportDiagnostic(params DiagnosticResult[] expectedDiagnosticResults)
    {
        foreach (var diagnostic in expectedDiagnosticResults)
        {
            ExpectedDiagnosticResults.Add(diagnostic);
        }

        return this;
    }

    public ProjectBuilder ShouldFixCodeWith(string codeFix) =>
        ShouldFixCodeWith(index: null, codeFix);

    public ProjectBuilder ShouldFixCodeWith(int? index, [StringSyntax("C#-test")] string codeFix)
    {
        ExpectedFixedCode = codeFix;
        CodeFixIndex = index;
        return this;
    }

    public ProjectBuilder WithTargetFramework(TargetFramework targetFramework)
    {
        TargetFramework = targetFramework;
        return this;
    }
}
