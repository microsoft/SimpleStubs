using Etg.SimpleStubs.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Etg.SimpleStubs.CodeGen
{
    class OrdinaryMethodStubber : IMethodStubber
    {
        public ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol, INamedTypeSymbol stubbedInterface)
        {
            if(!methodSymbol.IsOrdinaryMethod())
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
            if (methodSymbol.IsGenericMethod)
            {
                StatementSyntax stmtSyntax;
                if (methodSymbol.ReturnsVoid)
                {
                    stmtSyntax = SF.ParseStatement("\n");
                }
                else
                {
                    stmtSyntax = SF.ParseStatement($"return default({methodSymbol.ReturnType.GetFullyQualifiedName()});\n");
                }

                classDclr = classDclr.AddMembers(methodDclr.WithBody(SF.Block(stmtSyntax)));
            }
            else
            {
                string delegatePropertyName = NamingUtils.GetDelegatePropertyName(methodSymbol, stubbedInterface);
                string callDelegateStmt = $"{delegatePropertyName}({string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))});\n";
                if (!methodSymbol.ReturnsVoid)
                {
                    callDelegateStmt = callDelegateStmt.Insert(0, "return ");
                }

                classDclr = classDclr.AddMembers(
                    methodDclr.WithBody(SF.Block(SF.ParseStatement(callDelegateStmt))));
            }

            return classDclr;
        }
    }
}