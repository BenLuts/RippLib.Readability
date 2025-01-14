using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AnalyzersCodeFixProvider)), Shared]
    public class AnalyzersCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => [RuleIdentifiers.UseEmptyOverAny];

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);
            if (nodeToFix is null)
                return;

            var diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic is null)
                return;

            if (!Enum.TryParse(diagnostic.Properties.GetValueOrDefault("Data", ""), ignoreCase: false, out PreferAnyOverEmptyData data) || data is PreferAnyOverEmptyData.None)
                return;

            // If the so-called nodeToFix is a Name (most likely a method name such as 'Select' or 'Count'),
            // adjust it so that it refers to its InvocationExpression ancestor instead.
            if ((nodeToFix.IsKind(SyntaxKind.IdentifierName) || nodeToFix.IsKind(SyntaxKind.GenericName)) && !TryGetInvocationExpressionAncestor(ref nodeToFix))
                return;

            var title = "Optimize linq usage";
            switch (data)
            {
                case PreferAnyOverEmptyData.EmptyOverAny:
                    context.RegisterCodeFix(CodeAction.Create(title, ct => UseEmptyOverAny(context.Document, nodeToFix, ct), equivalenceKey: title), context.Diagnostics);
                    break;
            }
        }

        private static bool TryGetInvocationExpressionAncestor(ref SyntaxNode nodeToFix)
        {
            var node = nodeToFix;
            while (node is not null)
            {
                if (node.IsKind(SyntaxKind.InvocationExpression))
                {
                    nodeToFix = node;
                    return true;
                }
                node = node.Parent;
            }
            return false;
        }


        private static async Task<Document> UseEmptyOverAny(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
        {
            var expression = GetParentMemberExpression(nodeToFix);
            if (expression is null)
                return document;

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;
            var propertyAccess = generator.MemberAccessExpression(expression, "Count");

            editor.ReplaceNode(nodeToFix, propertyAccess);
            return editor.GetChangedDocument();
        }

        private static MemberAccessExpressionSyntax? GetMemberAccessExpression(SyntaxNode invocationExpressionSyntax)
        {
            if (invocationExpressionSyntax is not InvocationExpressionSyntax invocationExpression)
                return null;

            return invocationExpression.Expression as MemberAccessExpressionSyntax;
        }

        private static ExpressionSyntax? GetParentMemberExpression(SyntaxNode invocationExpressionSyntax)
        {
            var memberAccessExpression = GetMemberAccessExpression(invocationExpressionSyntax);
            if (memberAccessExpression is null)
                return null;

            return memberAccessExpression.Expression;
        }

    }
}
