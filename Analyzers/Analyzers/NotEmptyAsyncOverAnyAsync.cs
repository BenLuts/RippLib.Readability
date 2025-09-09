using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Analyzers.Constants;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NotEmptyAsyncOverAnyAsync : DiagnosticAnalyzer
{
    public const string DiagnosticId = AnalyzerDiagnosticIds.PreferNotEmptyAsyncOverAnyAsync;
    private static readonly LocalizableString _title = "Prefer .NotEmptyAsync() over .AnyAsync()";
    private static readonly LocalizableString _messageFormat =
        "Avoid using 'Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync()' to check for non-empty collections. Use RippLib.Readability.EFExtensions.NotEmptyAsync() instead.";
    private static readonly LocalizableString _description =
        "The use of .AnyAsync() is discouraged for readability. NotEmptyAsync() provides better intent and semantic clarity for checking if a collection has elements.";
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

        // Check for AnyAsync() from Entity Framework
        if (methodSymbol.Name == "AnyAsync" &&
            methodSymbol.ContainingNamespace.ToDisplayString() == "Microsoft.EntityFrameworkCore" &&
            methodSymbol.ContainingType?.Name == "EntityFrameworkQueryableExtensions")
        {
            // Check if this AnyAsync is negated through a logical not expression
            // We need to traverse up the syntax tree to account for await expressions
            var isNegated = IsNegatedAnyAsync(invocationExpr);

            // Only trigger for non-negated AnyAsync calls
            if (isNegated)
                return;

            // Only trigger for parameterless AnyAsync() (no predicate)
            if (methodSymbol.Parameters.Length == 1  &&
                 methodSymbol.Parameters[0].Type.Name == "CancellationToken")
            {
                var diagnostic = Diagnostic.Create(_rule, invocationExpr.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsNegatedAnyAsync(InvocationExpressionSyntax invocationExpr)
    {
        var current = invocationExpr.Parent;

        // Traverse up the syntax tree to handle await expressions
        while (current is not null)
        {
            switch (current)
            {
                case AwaitExpressionSyntax awaitExpr:
                    // Continue traversing up from the await expression
                    current = awaitExpr.Parent;
                    break;

                case PrefixUnaryExpressionSyntax unaryExpr when unaryExpr.IsKind(SyntaxKind.LogicalNotExpression):
                    // Found a logical not expression
                    return true;

                case ParenthesizedExpressionSyntax:
                    // Continue traversing up through parentheses
                    current = current.Parent;
                    break;

                default:
                    // Stop traversing if we hit any other kind of expression
                    return false;
            }
        }

        return false;
    }
}
