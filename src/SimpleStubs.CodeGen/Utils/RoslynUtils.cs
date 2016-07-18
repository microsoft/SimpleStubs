using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Etg.SimpleStubs.CodeGen.Utils
{
    internal static class RoslynUtils
    {
        public static UsingDirectiveSyntax UsingDirective(string nameSpace)
        {
            return SF.UsingDirective(SF.IdentifierName(nameSpace));
        }

        public static IEnumerable<MethodDeclarationSyntax> GetMethodDeclarations(TypeDeclarationSyntax interfaceNode)
        {
            return interfaceNode.DescendantNodes().OfType<MethodDeclarationSyntax>();
        }

        public static ParameterSyntax CreateParameter(string type, string name)
        {
            return SF.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), SF.IdentifierName(type),
                SF.Identifier(new SyntaxTriviaList().Add(SF.Space), name, new SyntaxTriviaList()), null);
        }

        public static BaseListSyntax BaseList(params string[] names)
        {
            return
                SF.BaseList(
                    SF.SeparatedList<BaseTypeSyntax>(names.Select(name => SF.SimpleBaseType(SF.IdentifierName(name)))));
        }

        public static List<ParameterSyntax> GetMethodParameterSyntaxList(IMethodSymbol methodSymbol)
        {
            var paramsSyntaxList = new List<ParameterSyntax>();
            foreach (IParameterSymbol param in methodSymbol.Parameters)
            {
                ParameterSyntax paramSyntax = SF.Parameter(SF.Identifier(param.Name))
                    .WithType(SF.ParseTypeName(param.Type.GetFullyQualifiedName()));

                if (param.RefKind == RefKind.Out)
                {
                    paramSyntax = paramSyntax.WithModifiers(SyntaxTokenList.Create(SF.Token(SyntaxKind.OutKeyword)));
                }
                else if (param.RefKind == RefKind.Ref)
                {
                    paramSyntax = paramSyntax.WithModifiers(SyntaxTokenList.Create(SF.Token(SyntaxKind.RefKeyword)));
                }

                paramsSyntaxList.Add(paramSyntax);
            }

            return paramsSyntaxList;
        }

        public static List<IMethodSymbol> GetAllMethods(INamedTypeSymbol interfaceType)
        {
            var methodsToStub = new List<IMethodSymbol>(interfaceType.GetMembers().OfType<IMethodSymbol>());
            methodsToStub.AddRange(GetAllInheritedMethods(interfaceType));
            return methodsToStub;
        }

        public static IEnumerable<IMethodSymbol> GetAllInheritedMethods(ITypeSymbol typeSymbol)
        {
            var methods = new List<IMethodSymbol>();
            if (typeSymbol.AllInterfaces.Any())
            {
                foreach (var baseInterfaceType in typeSymbol.AllInterfaces)
                {
                    methods.AddRange(baseInterfaceType.GetMembers().OfType<IMethodSymbol>());
                }
            }

            return methods;
        }

        public static SyntaxKind GetVisibilityKeyword(ISymbol stubbedInterface)
        {
            return stubbedInterface.DeclaredAccessibility ==
                   Accessibility.Internal
                ? SyntaxKind.InternalKeyword
                : SyntaxKind.PublicKeyword;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbolType">Can be a <see cref="INamedTypeSymbol"></see> or <see cref="IMethodSymbol"/>/></param>
        /// <param name="declarationSyntax">Can be a <see cref="ClassDeclarationSyntax"/> 
        /// or <see cref="MethodDeclarationSyntax"/> or <see cref="DelegateDeclarationSyntax"/></param>
        public static dynamic CopyGenericConstraints(dynamic symbolType, dynamic declarationSyntax)
        {
            if (!IsGenericType(symbolType))
            {
                return declarationSyntax;
            }
            foreach (ITypeParameterSymbol typeParameter in GetTypeParameters(symbolType))
            {
                if (!HasConstraints(typeParameter))
                {
                    continue;
                }

                TypeParameterConstraintClauseSyntax constraintSyntax = SF.TypeParameterConstraintClause(typeParameter.Name);
                if (typeParameter.HasReferenceTypeConstraint)
                {
                    constraintSyntax = constraintSyntax.AddConstraints(SF.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
                }
                if (typeParameter.HasValueTypeConstraint)
                {
                    constraintSyntax = constraintSyntax.AddConstraints(SF.ClassOrStructConstraint(SyntaxKind.StructConstraint));
                }
                IEnumerable<TypeParameterConstraintSyntax> typeConstraints =
                    typeParameter.ConstraintTypes.Select(symbol => SF.TypeConstraint(SF.IdentifierName(symbol.Name)));
                constraintSyntax = constraintSyntax.AddConstraints(typeConstraints.ToArray());

                if (typeParameter.HasConstructorConstraint)
                {
                    constraintSyntax = constraintSyntax.AddConstraints(SF.ConstructorConstraint());
                }

                declarationSyntax =
                    declarationSyntax.AddConstraintClauses(constraintSyntax);
            }
            return declarationSyntax;
        }

        private static bool HasConstraints(ITypeParameterSymbol typeParameter)
        {
            return typeParameter.HasConstructorConstraint || typeParameter.HasReferenceTypeConstraint ||
                   typeParameter.HasValueTypeConstraint || typeParameter.ConstraintTypes.Any();
        }

        private static bool IsGenericType(INamedTypeSymbol symbol)
        {
            return symbol.IsGenericType;
        }

        private static bool IsGenericType(IMethodSymbol symbol)
        {
            return symbol.IsGenericMethod;
        }

        private static IEnumerable<ITypeParameterSymbol> GetTypeParameters(INamedTypeSymbol symbol)
        {
            return symbol.TypeParameters;
        }

        private static IEnumerable<ITypeParameterSymbol> GetTypeParameters(IMethodSymbol symbol)
        {
            return symbol.TypeParameters;
        }
    }
}