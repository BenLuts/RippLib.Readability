﻿#if ROSLYN_3_8
using System.Collections.Immutable;
#endif
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Analyzers.Internals;

internal static class CompilationExtensions
{
#if ROSLYN_3_8
    public static ImmutableArray<INamedTypeSymbol> GetTypesByMetadataName(this Compilation compilation, string typeMetadataName)
    {
        var result = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        var symbol = compilation.Assembly.GetTypeByMetadataName(typeMetadataName);
        if (symbol != null)
        {
            result.Add(symbol);
        }

        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assemblySymbol == null)
                continue;

            symbol = assemblySymbol.GetTypeByMetadataName(typeMetadataName);
            if (symbol != null)
            {
                result.Add(symbol);
            }
        }

        return result.ToImmutable();
    }
#endif

    // Copy from https://github.com/dotnet/roslyn/blob/d2ff1d83e8fde6165531ad83f0e5b1ae95908289/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/CompilationExtensions.cs#L11-L68
    /// <summary>
    /// Gets a type by its metadata name to use for code analysis within a <see cref="Compilation"/>. This method
    /// attempts to find the "best" symbol to use for code analysis, which is the symbol matching the first of the
    /// following rules.
    ///
    /// <list type="number">
    ///   <item><description>
    ///     If only one type with the given name is found within the compilation and its referenced assemblies, that
    ///     type is returned regardless of accessibility.
    ///   </description></item>
    ///   <item><description>
    ///     If the current <paramref name="compilation"/> defines the symbol, that symbol is returned.
    ///   </description></item>
    ///   <item><description>
    ///     If exactly one referenced assembly defines the symbol in a manner that makes it visible to the current
    ///     <paramref name="compilation"/>, that symbol is returned.
    ///   </description></item>
    ///   <item><description>
    ///     Otherwise, this method returns <see langword="null"/>.
    ///   </description></item>
    /// </list>
    /// </summary>
    /// <param name="compilation">The <see cref="Compilation"/> to consider for analysis.</param>
    /// <param name="fullyQualifiedMetadataName">The fully-qualified metadata type name to find.</param>
    /// <returns>The symbol to use for code analysis; otherwise, <see langword="null"/>.</returns>
    public static INamedTypeSymbol GetBestTypeByMetadataName(this Compilation compilation, string fullyQualifiedMetadataName)
    {
        INamedTypeSymbol type = null;

        foreach (var currentType in compilation.GetTypesByMetadataName(fullyQualifiedMetadataName))
        {
            if (ReferenceEquals(currentType.ContainingAssembly, compilation.Assembly))
            {
                Debug.Assert(type is null);
                return currentType;
            }

            switch (currentType.GetResultantVisibility())
            {
                case SymbolVisibility.Public:
                case SymbolVisibility.Internal when currentType.ContainingAssembly.GivesAccessTo(compilation.Assembly):
                    break;

                default:
                    continue;
            }

            if (type is object)
            {
                // Multiple visible types with the same metadata name are present
                return null;
            }

            type = currentType;
        }

        return type;
    }

    // Copy from https://github.com/dotnet/roslyn/blob/d2ff1d83e8fde6165531ad83f0e5b1ae95908289/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/ISymbolExtensions.cs#L28-L73
    private static SymbolVisibility GetResultantVisibility(this ISymbol symbol)
    {
        // Start by assuming it's visible.
        var visibility = SymbolVisibility.Public;
        switch (symbol.Kind)
        {
            case SymbolKind.Alias:
                // Aliases are uber private.  They're only visible in the same file that they
                // were declared in.
                return SymbolVisibility.Private;
            case SymbolKind.Parameter:
                // Parameters are only as visible as their containing symbol
                return symbol.ContainingSymbol.GetResultantVisibility();
            case SymbolKind.TypeParameter:
                // Type Parameters are private.
                return SymbolVisibility.Private;
        }
        while (symbol is not null && symbol.Kind != SymbolKind.Namespace)
        {
            switch (symbol.DeclaredAccessibility)
            {
                // If we see anything private, then the symbol is private.
                case Accessibility.NotApplicable:
                case Accessibility.Private:
                    return SymbolVisibility.Private;
                // If we see anything internal, then knock it down from public to
                // internal.
                case Accessibility.Internal:
                case Accessibility.ProtectedAndInternal:
                    visibility = SymbolVisibility.Internal;
                    break;
                    // For anything else (Public, Protected, ProtectedOrInternal), the
                    // symbol stays at the level we've gotten so far.
            }
            symbol = symbol.ContainingSymbol;
        }
        return visibility;
    }

    private enum SymbolVisibility
    {
        Public,
        Internal,
        Private,
    }
}
