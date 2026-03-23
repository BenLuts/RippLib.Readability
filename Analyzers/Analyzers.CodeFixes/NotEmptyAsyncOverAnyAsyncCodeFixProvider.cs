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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotEmptyAsyncOverAnyAsyncCodeFixProvider)), Shared]
public class NotEmptyAsyncOverAnyAsyncCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [AnalyzerDiagnosticIds.PreferNotEmptyAsyncOverAnyAsync];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the invocation expression (i.e., query.AnyAsync())
        if (root.FindNode(diagnosticSpan) is not InvocationExpressionSyntax anyAsyncInvocation)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use .NotEmptyAsync() (and add using if needed)",
                createChangedDocument: c => ReplaceWithNotEmptyAsyncAndAddUsing(context.Document, anyAsyncInvocation, c),
                equivalenceKey: "UseNotEmptyAsync"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithNotEmptyAsyncAndAddUsing(
        Document document,
        InvocationExpressionSyntax anyAsyncInvocation,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

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

        var notEmptyAsyncAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            queryableExpression,
            SyntaxFactory.IdentifierName("NotEmptyAsync"));

        // Preserve the arguments (cancellation token if present)
        var notEmptyAsyncInvocation = SyntaxFactory.InvocationExpression(
            notEmptyAsyncAccess,
            newArgumentList);

        editor.ReplaceNode(anyAsyncInvocation, notEmptyAsyncInvocation);

        // Get the changed root after the .NotEmptyAsync() replacement
        var changedRoot = editor.GetChangedRoot();

        // Add using if missing
        if (changedRoot is CompilationUnitSyntax compilationUnit &&
            !compilationUnit.Usings.Any(u => u.Name.ToString() == "RippLib.Readability.EFExtensions"))
        {
            var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("RippLib.Readability.EFExtensions"));
            var newRoot = compilationUnit.AddUsings(newUsing);
            return document.WithSyntaxRoot(newRoot);
        }

        return document.WithSyntaxRoot(changedRoot);
    }
}
