using Analyzers.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ContainsNoneAsyncOverNotAnyAsyncCodeFixProvider)), Shared]
public class ContainsNoneAsyncOverNotAnyAsyncCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [AnalyzerDiagnosticIds.PreferContainsNoneAsyncOverNotAnyAsync];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (root.FindNode(diagnosticSpan) is not InvocationExpressionSyntax anyAsyncInvocation)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use .ContainsNoneAsync() (and add using if needed)",
                createChangedDocument: c => ReplaceWithContainsNoneAsyncAndAddUsing(context.Document, anyAsyncInvocation, c),
                equivalenceKey: "UseContainsNoneAsync"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithContainsNoneAsyncAndAddUsing(
        Document document,
        InvocationExpressionSyntax anyAsyncInvocation,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        var negatingExpr = FindNegatingExpression(anyAsyncInvocation);
        if (negatingExpr is null)
            return document;

        // Detect invocation style and extract queryable expression
        ExpressionSyntax queryableExpression;
        ArgumentListSyntax newArgumentList;
        if (anyAsyncInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // Reduced extension form: query.AnyAsync(...)
            queryableExpression = memberAccess.Expression;
            newArgumentList = anyAsyncInvocation.ArgumentList;
        }
        else
        {
            // Static form: EntityFrameworkQueryableExtensions.AnyAsync(query, ...)
            var args = anyAsyncInvocation.ArgumentList.Arguments;
            queryableExpression = args[0].Expression;
            newArgumentList = anyAsyncInvocation.ArgumentList.WithArguments(
                SyntaxFactory.SeparatedList(args.Skip(1)));
        }

        // Build: query.ContainsNoneAsync(predicate, ct)
        var containsNoneAsyncAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            queryableExpression,
            SyntaxFactory.IdentifierName("ContainsNoneAsync"));

        var containsNoneAsyncInvocation = SyntaxFactory.InvocationExpression(
            containsNoneAsyncAccess,
            newArgumentList);

        // Build: await query.ContainsNoneAsync(predicate, ct) — replacing the whole !await ... expression
        var awaitContainsNoneAsync = SyntaxFactory.AwaitExpression(containsNoneAsyncInvocation)
            .WithTriviaFrom(negatingExpr);

        editor.ReplaceNode(negatingExpr, awaitContainsNoneAsync);

        var changedRoot = editor.GetChangedRoot();

        if (changedRoot is CompilationUnitSyntax compilationUnit &&
            !compilationUnit.Usings.Any(u => u.Name.ToString() == "RippLib.Readability.EFExtensions"))
        {
            var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("RippLib.Readability.EFExtensions"));
            return document.WithSyntaxRoot(compilationUnit.AddUsings(newUsing));
        }

        return document.WithSyntaxRoot(changedRoot);
    }

    private static PrefixUnaryExpressionSyntax FindNegatingExpression(InvocationExpressionSyntax invocationExpr)
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
