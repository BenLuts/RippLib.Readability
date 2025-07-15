using Analyzers.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Data;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EmptyOverNotAny : DiagnosticAnalyzer
{
    public const string DiagnosticId = AnalyzerDiagnosticIds.PreferEmptyOverNotAny;
    private static readonly LocalizableString _title = "Prefer .Empty() over !.Any()";
    private static readonly LocalizableString _messageFormat = "Avoid using '!System.Linq.Enumerable.Any()' to check for none empty collections. Use RippLib.Readability.Empty() instead.";
    private static readonly LocalizableString _description =
        "The use of !.Any() is discouraged for readability. Empty() has a built-in null check & uses the most performant check based on the type of the collection.";
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
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var semanticModel = context.SemanticModel;
            var symbolInfo = semanticModel.GetSymbolInfo(invocationExpr);

            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return;

            if (methodSymbol.Name == "Any" &&
                methodSymbol.ContainingNamespace.ToDisplayString() == "System.Linq" &&
                methodSymbol.ContainingType?.Name == "Enumerable")
            {
                var diagnostic = Diagnostic.Create(_rule, unaryExpr.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
