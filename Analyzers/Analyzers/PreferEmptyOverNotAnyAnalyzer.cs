using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferEmptyOverNotAnyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _useEmptyInsteadOfNotAny = new(
    RuleIdentifiers.UseEmptyOverAny,
    title: "Use 'Empty()' instead of 'Any()'",
    messageFormat: "Use 'Empty()' instead of 'Any()'",
    "Readability",
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description: "");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_useEmptyInsteadOfNotAny];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(ctx =>
        {
            var typeSymbol = ctx.Compilation.GetTypeByMetadataName("RippLib.Readability.EnumerableExtensions");
            if (typeSymbol == null)
                return;

            var analyzerContext = new AnalyzerContext(ctx.Compilation);
            if (analyzerContext.IsValid)
            {
                ctx.RegisterOperationAction(analyzerContext.AnalyzeInvocation, OperationKind.Invocation);
            }
        });
    }

    private sealed class AnalyzerContext
    {
        private List<INamedTypeSymbol> ExtensionMethodOwnerTypes { get; } = [];
        private INamedTypeSymbol? EnumerableSymbol { get; set; }
        private INamedTypeSymbol? QueryableSymbol { get; set; }
        private INamedTypeSymbol? ICollectionOfTSymbol { get; set; }
        private INamedTypeSymbol? ICollectionSymbol { get; set; }
        private INamedTypeSymbol? IReadOnlyCollectionOfTSymbol { get; set; }

        public bool IsValid => ExtensionMethodOwnerTypes.Count > 0;

        public AnalyzerContext(Compilation compilation)
        {
            EnumerableSymbol = compilation.GetBestTypeByMetadataName("System.Linq.Enumerable");
            QueryableSymbol = compilation.GetBestTypeByMetadataName("System.Linq.Queryable");
            IReadOnlyCollectionOfTSymbol = compilation.GetBestTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
            ICollectionOfTSymbol = compilation.GetBestTypeByMetadataName("System.Collections.Generic.ICollection`1");
            ICollectionSymbol = compilation.GetBestTypeByMetadataName("System.Collections.ICollection");
            ExtensionMethodOwnerTypes.AddIfNotNull(EnumerableSymbol);
            ExtensionMethodOwnerTypes.AddIfNotNull(QueryableSymbol);
        }


        public void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var operation = (IInvocationOperation)context.Operation;
            if (operation.Arguments.Length == 0)
                return;

            var method = operation.TargetMethod;
            if (!ExtensionMethodOwnerTypes.Contains(method.ContainingType))
                return;

            UseEmptyInsteadOfNotAny(context, operation);
        }

        private void UseEmptyInsteadOfNotAny(OperationAnalysisContext context, IInvocationOperation operation)
        {
            if (operation.TargetMethod.Name == "Any" && operation.TargetMethod.ContainingType.IsEqualTo(EnumerableSymbol))
            {
                //// Any(_ => true)
                //if (operation.Arguments.Length >= 2)
                //    return;

                var operandType = operation.Arguments[0].Value.GetActualType();
                if (operandType is null)
                    return;

                var t = (operandType as INamedTypeSymbol).Arity;
                var implementedInterfaces = operandType.GetAllInterfacesIncludingThis().Select(i => i.OriginalDefinition);
                if (implementedInterfaces.Any(i => i.IsEqualTo(ICollectionOfTSymbol) || i.IsEqualTo(ICollectionSymbol) || i.IsEqualTo(IReadOnlyCollectionOfTSymbol)))
                {
                    context.ReportDiagnostic(PreferEmptyOverNotAnyAnalyzer._useEmptyInsteadOfNotAny, operation);
                }
            }
        }
    }

}
