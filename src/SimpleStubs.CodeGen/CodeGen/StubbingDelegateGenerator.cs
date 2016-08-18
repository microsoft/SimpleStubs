using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Etg.SimpleStubs.CodeGen.Utils;

namespace Etg.SimpleStubs.CodeGen
{
    using Microsoft.CodeAnalysis.CSharp;
    using System.Collections.Generic;
    using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal class StubbingDelegateGenerator : IMethodStubber
    {
        public ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol,
            INamedTypeSymbol stubbedInterface)
        {
            if (methodSymbol.IsPropertyAccessor() || methodSymbol.IsOrdinaryMethod())
            {
                string delegateTypeName = NamingUtils.GetDelegateTypeName(methodSymbol, stubbedInterface);
                string setupMethodName = NamingUtils.GetSetupMethodName(methodSymbol);

                DelegateDeclarationSyntax delegateDclr = GenerateDelegateDclr(methodSymbol, delegateTypeName,
                    stubbedInterface);
                MethodDeclarationSyntax propDclr = GenerateSetupMethod(methodSymbol, setupMethodName, delegateTypeName,
                    stubbedInterface, classDclr);
                classDclr = classDclr.AddMembers(delegateDclr, propDclr);
            }

            return classDclr;
        }

        private static MethodDeclarationSyntax GenerateSetupMethod(IMethodSymbol methodSymbol, string setupMethodName, string delegateTypeName,
            INamedTypeSymbol stubbedInterface,
            ClassDeclarationSyntax stub)
        {
            SyntaxKind visibility = RoslynUtils.GetVisibilityKeyword(stubbedInterface);
            MethodDeclarationSyntax methodDclr = SF.MethodDeclaration(SF.ParseTypeName(stub.Identifier.Text), setupMethodName)
                .AddModifiers(SF.Token(visibility)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken))
                .AddParameterListParameters(
                    SF.Parameter(SF.Identifier("del")).WithType(SF.ParseTypeName(delegateTypeName)))
                .WithBody(SF.Block(
                    SF.ParseStatement("_stubs.SetMethodStub(del);\n"),
                    SF.ParseStatement("return this;\n")
                    ))
                .WithSemicolonToken(SF.Token(SyntaxKind.None));

            return RoslynUtils.CopyGenericConstraints(methodSymbol, methodDclr);
        }

        private static DelegateDeclarationSyntax GenerateDelegateDclr(IMethodSymbol methodSymbol, string delegateName,
            INamedTypeSymbol stubbedInterface)
        {
            SyntaxKind visibility = RoslynUtils.GetVisibilityKeyword(stubbedInterface);
            List<ParameterSyntax> paramsSyntaxList = RoslynUtils.GetMethodParameterSyntaxList(methodSymbol);
            DelegateDeclarationSyntax delegateDeclaration = SF.DelegateDeclaration(SF.ParseTypeName(methodSymbol.ReturnType.GetFullyQualifiedName()),
                delegateName)
                .AddModifiers(SF.Token(visibility)).AddParameterListParameters(paramsSyntaxList.ToArray());

            delegateDeclaration = RoslynUtils.CopyGenericConstraints(methodSymbol, delegateDeclaration);
            return delegateDeclaration;
        }
    }
}