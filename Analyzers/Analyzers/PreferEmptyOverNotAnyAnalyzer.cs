using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferEmptyOverNotAnyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ANY001";
    private static readonly LocalizableString _title = "Avoid using the Linq Any method";
    private static readonly LocalizableString _messageFormat = "Use .Empty() instead of Any()";
    private static readonly LocalizableString _description = "The Empty() method is more readable and has a build in NULL check.";
    private const string Category = "Readability";

    private static readonly DiagnosticDescriptor _rule = new(
        DiagnosticId, _title, _messageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: _description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Look for method calls named Any.
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || !memberAccess.Name.ToString().StartsWith("Any"))
            return;

        // Inspect the arguments of the Any method
        if (invocation.ArgumentList.Arguments.Count > 0)
            return;

        var diagnostic = Diagnostic.Create(_rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
