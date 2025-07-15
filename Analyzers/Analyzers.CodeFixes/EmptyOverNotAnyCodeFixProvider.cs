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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EmptyOverNotAnyCodeFixProvider)), Shared]
public class EmptyOverNotAnyCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [AnalyzerDiagnosticIds.PreferEmptyOverNotAny];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the unary expression (i.e., !list.Any())
        if (root.FindNode(diagnosticSpan) is not PrefixUnaryExpressionSyntax notAnyExpr)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use .Empty() (and add using if needed)",
                createChangedDocument: c => ReplaceWithEmptyAndAddUsing(context.Document, notAnyExpr, c),
                equivalenceKey: "UseEmptyAndAddUsing"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithEmptyAndAddUsing(
        Document document,
        PrefixUnaryExpressionSyntax notAnyExpr,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        // Replace !list.Any() with list.Empty()
        if (notAnyExpr.Operand is InvocationExpressionSyntax invocation &&
            invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var emptyAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccess.Expression,
                SyntaxFactory.IdentifierName("Empty"));

            var emptyInvocation = SyntaxFactory.InvocationExpression(emptyAccess);
            editor.ReplaceNode(notAnyExpr, emptyInvocation);
        }

        // Get the changed root after the .Empty() replacement
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
