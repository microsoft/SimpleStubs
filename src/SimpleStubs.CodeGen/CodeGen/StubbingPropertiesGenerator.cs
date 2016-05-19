using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Etg.SimpleStubs.CodeGen.Utils;

namespace Etg.SimpleStubs.CodeGen
{
    using Microsoft.CodeAnalysis.CSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    class StubbingPropertiesGenerator : IMethodStubber
    {
        public ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol, 
            INamedTypeSymbol stubbedInterface)
        {
            if (methodSymbol.IsPropertyAccessor() || methodSymbol.IsOrdinaryMethod())
            {
                if (!methodSymbol.IsGenericMethod)
                {
                    string delegatePropertyName = NamingUtils.GetDelegatePropertyName(methodSymbol, stubbedInterface);
                    string delegateTypeName = NamingUtils.GetDelegateTypeName(delegatePropertyName);

                    DelegateDeclarationSyntax delegateDclr = GenerateDelegateDclr(methodSymbol, delegateTypeName, stubbedInterface);
                    PropertyDeclarationSyntax propDclr = GenerateDelegatePropDclr(
                        delegatePropertyName, delegateTypeName, stubbedInterface);
                    classDclr = classDclr.AddMembers(delegateDclr, propDclr);
                }
            }

            return classDclr;
        }

        private static PropertyDeclarationSyntax GenerateDelegatePropDclr(string delegatePropertyName, 
            string delegateName, INamedTypeSymbol stubbedInterface)
        {
            SyntaxKind visibility = RoslynUtils.GetVisibilityKeyword(stubbedInterface);
            return SF.PropertyDeclaration(SF.ParseTypeName(delegateName), delegatePropertyName)
                .AddModifiers(SF.Token(visibility)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
        }

        private static DelegateDeclarationSyntax GenerateDelegateDclr(IMethodSymbol methodSymbol, string delegateName, 
            INamedTypeSymbol stubbedInterface)
        {
            SyntaxKind visibility = RoslynUtils.GetVisibilityKeyword(stubbedInterface);
            List<ParameterSyntax> paramsSyntaxList = RoslynUtils.GetMethodParameterSyntaxList(methodSymbol);
            return SF.DelegateDeclaration(SF.ParseTypeName(methodSymbol.ReturnType.GetFullyQualifiedName()), delegateName)
                .AddModifiers(SF.Token(visibility)).AddParameterListParameters(paramsSyntaxList.ToArray());
        }
    }
}