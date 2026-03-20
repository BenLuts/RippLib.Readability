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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EmptyAsyncOverNotAnyAsyncCodeFixProvider)), Shared]
public class EmptyAsyncOverNotAnyAsyncCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [AnalyzerDiagnosticIds.PreferEmptyAsyncOverNotAnyAsync];

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
                title: "Use .EmptyAsync() (and add using if needed)",
                createChangedDocument: c => ReplaceWithEmptyAsyncAndAddUsing(context.Document, anyAsyncInvocation, c),
                equivalenceKey: "UseEmptyAsync"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithEmptyAsyncAndAddUsing(
        Document document,
        InvocationExpressionSyntax anyAsyncInvocation,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        if (anyAsyncInvocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return document;

        var negatingExpr = FindNegatingExpression(anyAsyncInvocation);
        if (negatingExpr is null)
            return document;

        // Build: query.EmptyAsync(args)
        var emptyAsyncAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            memberAccess.Expression,
            SyntaxFactory.IdentifierName("EmptyAsync"));

        var emptyAsyncInvocation = SyntaxFactory.InvocationExpression(
            emptyAsyncAccess,
            anyAsyncInvocation.ArgumentList);

        // Build: await query.EmptyAsync(args) — replacing the whole !await ... expression
        var awaitEmptyAsync = SyntaxFactory.AwaitExpression(emptyAsyncInvocation)
            .WithTriviaFrom(negatingExpr);

        editor.ReplaceNode(negatingExpr, awaitEmptyAsync);

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
