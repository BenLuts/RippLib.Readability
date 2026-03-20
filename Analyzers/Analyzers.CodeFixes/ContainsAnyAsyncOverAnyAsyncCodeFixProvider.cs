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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ContainsAnyAsyncOverAnyAsyncCodeFixProvider)), Shared]
public class ContainsAnyAsyncOverAnyAsyncCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [AnalyzerDiagnosticIds.PreferContainsAnyAsyncOverAnyAsync];

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
                title: "Use .ContainsAnyAsync() (and add using if needed)",
                createChangedDocument: c => ReplaceWithContainsAnyAsyncAndAddUsing(context.Document, anyAsyncInvocation, c),
                equivalenceKey: "UseContainsAnyAsync"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithContainsAnyAsyncAndAddUsing(
        Document document,
        InvocationExpressionSyntax anyAsyncInvocation,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        if (anyAsyncInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var containsAnyAsyncAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccess.Expression,
                SyntaxFactory.IdentifierName("ContainsAnyAsync"));

            var containsAnyAsyncInvocation = SyntaxFactory.InvocationExpression(
                containsAnyAsyncAccess,
                anyAsyncInvocation.ArgumentList);

            editor.ReplaceNode(anyAsyncInvocation, containsAnyAsyncInvocation);
        }

        var changedRoot = editor.GetChangedRoot();

        if (changedRoot is CompilationUnitSyntax compilationUnit &&
            !compilationUnit.Usings.Any(u => u.Name.ToString() == "RippLib.Readability.EFExtensions"))
        {
            var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("RippLib.Readability.EFExtensions"));
            return document.WithSyntaxRoot(compilationUnit.AddUsings(newUsing));
        }

        return document.WithSyntaxRoot(changedRoot);
    }
}
