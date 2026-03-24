using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Analyzers.Constants;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ContainsAnyAsyncOverAnyAsync : DiagnosticAnalyzer
{
    public const string DiagnosticId = AnalyzerDiagnosticIds.PreferContainsAnyAsyncOverAnyAsync;
    private static readonly LocalizableString _title = "Prefer .ContainsAnyAsync() over .AnyAsync(predicate)";
    private static readonly LocalizableString _messageFormat =
        "Avoid using 'Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(predicate)' to check for matching elements. Use RippLib.Readability.EFExtensions.ContainsAnyAsync() instead.";
    private static readonly LocalizableString _description =
        "The use of .AnyAsync(predicate) is discouraged for readability. ContainsAnyAsync() provides better intent and semantic clarity for checking if a collection contains matching elements.";
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

    private const string CompanionTypeName = "RippLib.Readability.EFExtensions.QueryableExtensions";

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationContext =>
        {
            if (compilationContext.Compilation.GetTypeByMetadataName(CompanionTypeName) is null)
                return;

            compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpr);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        // Normalize to the underlying method definition so we handle both
        // reduced extension calls (query.AnyAsync(predicate, ct)) and static-form calls
        // (EntityFrameworkQueryableExtensions.AnyAsync(query, predicate, ct)).
        var baseMethod = methodSymbol.ReducedFrom ?? methodSymbol;

        if (baseMethod.Name != "AnyAsync" ||
            baseMethod.ContainingNamespace.ToDisplayString() != "Microsoft.EntityFrameworkCore" ||
            baseMethod.ContainingType?.Name != "EntityFrameworkQueryableExtensions")
            return;

        // Only match the predicate overload: AnyAsync(source, predicate, CancellationToken)
        var parameters = baseMethod.Parameters;

        var hasPredicateParameter = parameters.Any(p =>
            p.Type.ToDisplayString().Contains("System.Linq.Expressions.Expression"));

        if (!hasPredicateParameter)
            return;

        if (parameters[parameters.Length - 1].Type.Name != "CancellationToken")
            return;

        if (IsNegatedAnyAsync(invocationExpr))
            return;

        context.ReportDiagnostic(Diagnostic.Create(_rule, invocationExpr.GetLocation()));
    }

    private static bool IsNegatedAnyAsync(InvocationExpressionSyntax invocationExpr)
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
}
