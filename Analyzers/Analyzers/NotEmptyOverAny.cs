using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Analyzers.Constants;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NotEmptyOverAny : DiagnosticAnalyzer
{
    public const string DiagnosticId = AnalyzerDiagnosticIds.PreferNotEmptyOverAny;
    private static readonly LocalizableString _title = "Prefer .NotEmpty() over .Any()";
    private static readonly LocalizableString _messageFormat = "Avoid using 'System.Linq.Enumerable.Any()' to check for none empty collections. Use RippLib.Readability.NotEmpty() instead.";
    private static readonly LocalizableString _description = 
        "The use of .Any() is discouraged for readability. NotEmpty() has a built-in null check & uses the most performant check based on the type of the collection.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor _rule = new(
        DiagnosticId,
        _title,
        _messageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: _description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node.Parent is PrefixUnaryExpressionSyntax unaryExpr &&
            unaryExpr.IsKind(SyntaxKind.LogicalNotExpression))
            return;

        var invocationExpr = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpr);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        if (methodSymbol.Name == "Any" &&
            methodSymbol.ContainingNamespace.ToDisplayString() == "System.Linq" &&
            methodSymbol.ContainingType?.Name == "Enumerable" &&
            methodSymbol.Parameters.Length == 0)
        {
            var diagnostic = Diagnostic.Create(_rule, invocationExpr.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
