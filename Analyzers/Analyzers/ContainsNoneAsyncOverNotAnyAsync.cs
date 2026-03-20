using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Analyzers.Constants;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ContainsNoneAsyncOverNotAnyAsync : DiagnosticAnalyzer
{
    public const string DiagnosticId = AnalyzerDiagnosticIds.PreferContainsNoneAsyncOverNotAnyAsync;
    private static readonly LocalizableString _title = "Prefer .ContainsNoneAsync() over !.AnyAsync(predicate)";
    private static readonly LocalizableString _messageFormat =
        "Avoid using '!Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(predicate)' to check for no matching elements. Use RippLib.Readability.EFExtensions.ContainsNoneAsync() instead.";
    private static readonly LocalizableString _description =
        "The use of !.AnyAsync(predicate) is discouraged for readability. ContainsNoneAsync() provides better intent and semantic clarity for checking if a collection contains no matching elements.";
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

        if (methodSymbol.Name != "AnyAsync" ||
            methodSymbol.ContainingNamespace.ToDisplayString() != "Microsoft.EntityFrameworkCore" ||
            methodSymbol.ContainingType?.Name != "EntityFrameworkQueryableExtensions")
            return;

        // Only match the predicate overload: AnyAsync(predicate, CancellationToken)
        if (methodSymbol.Parameters.Length != 2 ||
            methodSymbol.Parameters[1].Type.Name != "CancellationToken")
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
