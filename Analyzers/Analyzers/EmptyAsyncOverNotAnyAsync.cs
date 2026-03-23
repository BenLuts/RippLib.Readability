using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Analyzers.Constants;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EmptyAsyncOverNotAnyAsync : DiagnosticAnalyzer
{
    public const string DiagnosticId = AnalyzerDiagnosticIds.PreferEmptyAsyncOverNotAnyAsync;
    private static readonly LocalizableString _title = "Prefer .EmptyAsync() over !.AnyAsync()";
    private static readonly LocalizableString _messageFormat =
        "Avoid using '!Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync()' to check for empty collections. Use RippLib.Readability.EFExtensions.EmptyAsync() instead.";
    private static readonly LocalizableString _description =
        "The use of !.AnyAsync() is discouraged for readability. EmptyAsync() provides better intent and semantic clarity for checking if a collection has no elements.";
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
        var invocationExpr = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpr);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        // Normalize to the original extension method so invocation style (reduced vs static)
        // does not affect parameter inspection.
        var originalMethod = methodSymbol.ReducedFrom ?? methodSymbol;

        if (originalMethod.Name != "AnyAsync" ||
            originalMethod.ContainingNamespace.ToDisplayString() != "Microsoft.EntityFrameworkCore" ||
            originalMethod.ContainingType?.Name != "EntityFrameworkQueryableExtensions")
            return;

        var parameters = originalMethod.Parameters;

        // Only match the overload without a predicate:
        // EntityFrameworkQueryableExtensions.AnyAsync(source, CancellationToken)
        if (!(parameters.Length == 2 &&
              parameters[1].Type.Name == "CancellationToken"))
            return;

        if (!IsNegatedAnyAsync(invocationExpr))
            return;

        context.ReportDiagnostic(Diagnostic.Create(_rule, invocationExpr.GetLocation()));
    }

    internal static bool IsNegatedAnyAsync(InvocationExpressionSyntax invocationExpr)
    {
        var current = invocationExpr.Parent;
        while (current is not null)
        {
            switch (current)
            {
                case AwaitExpressionSyntax:
                case ParenthesizedExpressionSyntax:
                    current = current.Parent;
                    break;
                case PrefixUnaryExpressionSyntax unaryExpr when unaryExpr.IsKind(SyntaxKind.LogicalNotExpression):
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }

    internal static PrefixUnaryExpressionSyntax FindNegatingExpression(InvocationExpressionSyntax invocationExpr)
    {
        var current = invocationExpr.Parent;
        while (current is not null)
        {
            switch (current)
            {
                case AwaitExpressionSyntax:
                case ParenthesizedExpressionSyntax:
                    current = current.Parent;
                    break;
                case PrefixUnaryExpressionSyntax unaryExpr when unaryExpr.IsKind(SyntaxKind.LogicalNotExpression):
                    return unaryExpr;
                default:
                    return null;
            }
        }
        return null;
    }
}
