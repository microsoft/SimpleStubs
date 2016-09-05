using Etg.SimpleStubs.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Etg.SimpleStubs.CodeGen
{
    internal class OrdinaryMethodStubber : IMethodStubber
    {
        public ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol,
            INamedTypeSymbol stubbedInterface)
        {
            if (!methodSymbol.IsOrdinaryMethod())
            {
                return classDclr;
            }

            MethodDeclarationSyntax methodDclr = SF.MethodDeclaration(
                SF.ParseTypeName(methodSymbol.ReturnType.GetFullyQualifiedName()), methodSymbol.GetGenericName());
            methodDclr = methodDclr.WithParameterList(methodDclr.ParameterList.AddParameters(
                RoslynUtils.GetMethodParameterSyntaxList(methodSymbol).ToArray()));
            methodDclr = methodDclr.WithSemicolonToken(SF.Token(SyntaxKind.None))
                .WithExplicitInterfaceSpecifier(
                    SF.ExplicitInterfaceSpecifier(
                        SF.IdentifierName(methodSymbol.GetContainingInterfaceGenericQualifiedName())));

            string delegateTypeName = NamingUtils.GetDelegateTypeName(methodSymbol, stubbedInterface);
            string parameters = StubbingUtils.FormatParameters(methodSymbol);

            string callDelegateStmt = StubbingUtils.GenerateInvokeDelegateStmt(delegateTypeName, methodSymbol.GetGenericName(), parameters);
            if (!methodSymbol.ReturnsVoid)
            {
                callDelegateStmt = callDelegateStmt.Insert(0, "return ");
            }

            classDclr = classDclr.AddMembers(
                methodDclr.WithBody(SF.Block(SF.ParseStatement(callDelegateStmt))));

            return classDclr;
        }
    }
}