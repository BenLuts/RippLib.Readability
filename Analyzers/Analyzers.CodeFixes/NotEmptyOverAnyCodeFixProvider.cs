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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotEmptyOverAnyCodeFixProvider)), Shared]
public class NotEmptyOverAnyCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [AnalyzerDiagnosticIds.PreferNotEmptyOverAny];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the invocation expression (i.e., list.Any())
        if (root.FindNode(diagnosticSpan) is not InvocationExpressionSyntax anyInvocation)
            return;

        // Main code fix: Replace .Any() with .NotEmpty() and add using if missing
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use .NotEmpty() (and add using if needed)",
                createChangedDocument: c => ReplaceWithNotEmptyAndAddUsing(context.Document, anyInvocation, c),
                equivalenceKey: "UseNotEmptyAndAddUsing"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithNotEmptyAndAddUsing(
        Document document,
        InvocationExpressionSyntax anyInvocation,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        // Replace .Any() with .NotEmpty()
        if (anyInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var notEmptyAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccess.Expression,
                SyntaxFactory.IdentifierName("NotEmpty"));

            var notEmptyInvocation = SyntaxFactory.InvocationExpression(notEmptyAccess);
            editor.ReplaceNode(anyInvocation, notEmptyInvocation);
        }

        // Get the changed root after the .NotEmpty() replacement
        var changedRoot = editor.GetChangedRoot();

        // Add using if missing
        if (changedRoot is CompilationUnitSyntax compilationUnit &&
            !compilationUnit.Usings.Any(u => u.Name.ToString() == "RippLib.Readability"))
        {
            var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("RippLib.Readability"));
            var newRoot = compilationUnit.AddUsings(newUsing);
            return document.WithSyntaxRoot(newRoot);
        }

        return document.WithSyntaxRoot(changedRoot);
    }
}
