using System.Linq;
using Microsoft.CodeAnalysis;

namespace Analyzers;

internal static class MethodSymbolExtensions
{
    private static readonly string[] _msTestNamespaceParts = ["Microsoft", "VisualStudio", "TestTools", "UnitTesting"];
    private static readonly string[] _nunitNamespaceParts = ["NUnit", "Framework"];
    private static readonly string[] _xunitNamespaceParts = ["Xunit"];

    public static bool IsInterfaceImplementation(this IMethodSymbol symbol)
    {
        if (symbol.ExplicitInterfaceImplementations.Length > 0)
            return true;

        return ((ISymbol)symbol).IsInterfaceImplementation();
    }

    public static bool IsInterfaceImplementation(this IPropertySymbol symbol)
    {
        if (symbol.ExplicitInterfaceImplementations.Length > 0)
            return true;

        return ((ISymbol)symbol).IsInterfaceImplementation();
    }

    public static bool IsInterfaceImplementation(this IEventSymbol symbol)
    {
        if (symbol.ExplicitInterfaceImplementations.Length > 0)
            return true;

        return ((ISymbol)symbol).IsInterfaceImplementation();
    }

    private static bool IsInterfaceImplementation(this ISymbol symbol)
    {
        return symbol.GetImplementingInterfaceSymbol() is not null;
    }

    public static IMethodSymbol GetImplementingInterfaceSymbol(this IMethodSymbol symbol)
    {
        if (symbol.ExplicitInterfaceImplementations.Any())
            return symbol.ExplicitInterfaceImplementations.First();

        return (IMethodSymbol)((ISymbol)symbol).GetImplementingInterfaceSymbol();
    }

    private static ISymbol GetImplementingInterfaceSymbol(this ISymbol symbol)
    {
        if (symbol.ContainingType is null)
            return null;

        return symbol.ContainingType.AllInterfaces
            .SelectMany(@interface => @interface.GetMembers())
            .FirstOrDefault(interfaceMember => SymbolEqualityComparer.Default.Equals(symbol, symbol.ContainingType.FindImplementationForInterfaceMember(interfaceMember)));
    }

    public static bool IsUnitTestMethod(this IMethodSymbol methodSymbol)
    {
        var attributes = methodSymbol.GetAttributes();
        foreach (var attribute in attributes)
        {
            var type = attribute.AttributeClass;
            while (type is not null)
            {
                var ns = type.ContainingNamespace;
                if (ns.IsNamespace(_msTestNamespaceParts) ||
                    ns.IsNamespace(_nunitNamespaceParts) ||
                    ns.IsNamespace(_xunitNamespaceParts))
                {
                    return true;
                }

                type = type.BaseType;
            }
        }

        return false;
    }
}
