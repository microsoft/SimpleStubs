using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace Etg.SimpleStubs.CodeGen.Utils
{
    internal static class RoslynExtensions
    {
        public static SymbolDisplayFormat QualifiedFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        public static bool IsEvent(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsEventAdd() || methodSymbol.IsEventRemove();
        }

        public static bool IsEventAdd(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.EventAdd;
        }

        public static bool IsEventRemove(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.EventRemove;
        }

        public static bool IsPropertyAccessor(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsPropertyGetter() || methodSymbol.IsPropertySetter();
        }

        public static bool IsPropertySetter(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.PropertySet;
        }

        public static bool IsPropertyGetter(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.PropertyGet;
        }

        public static string GetGenericName(this IMethodSymbol methodSymbol)
        {
            string name = methodSymbol.Name;
            if (methodSymbol.IsGenericMethod)
            {
                name = $"{name}<{string.Join(",", methodSymbol.TypeParameters.Select(p => p.Name))}>";
            }
            return name;
        }

        public static string GetContainingInterfaceGenericQualifiedName(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public static string GetGenericName(this ISymbol namedType)
        {
            return namedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }

        public static string GetFullyQualifiedName(this ITypeSymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public static string GetQualifiedName(this ITypeSymbol symbol)
        {
            return
                symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries)
                    .Last();
        }

        public static string GetMinimallyQualifiedName(this ITypeSymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }

        public static bool IsOrdinaryMethod(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.Ordinary;
        }

        public static bool IsPublic(this TypeDeclarationSyntax typeDclr)
        {
            return
                typeDclr.Modifiers.Any(
                    modifier => modifier.RawKind.Equals(SyntaxFactory.Token(SyntaxKind.PublicKeyword).RawKind));
        }

        public static bool IsInternal(this TypeDeclarationSyntax typeDclr)
        {
            var nonInternalModifiers = new List<int>
            {
                SyntaxFactory.Token(SyntaxKind.PublicKeyword).RawKind,
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword).RawKind,
                SyntaxFactory.Token(SyntaxKind.ProtectedKeyword).RawKind
            };
            return !typeDclr.Modifiers.Any(modifier => nonInternalModifiers.Contains(modifier.RawKind));
        }
    }
}