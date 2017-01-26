using Etg.SimpleStubs.CodeGen.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Etg.SimpleStubs.CodeGen
{
    internal class FluentConfigurationStubber : IFluentConfigurationStubber
    {
        public ClassDeclarationSyntax StubMockBehaviorConfigurationMethod(INamedTypeSymbol interfaceType, ClassDeclarationSyntax classDclr)
        {
            string interfaceName = interfaceType.GetGenericName();
            string stubName = NamingUtils.GetStubName(interfaceName);

            MethodDeclarationSyntax methodDclr = SF.MethodDeclaration(SF.ParseTypeName(stubName), "WithDefaultBehavior")
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(SF.Parameter(SF.Identifier("mockBehavior")).WithType(SF.ParseTypeName("MockBehavior")))
                .AddBodyStatements(SF.ExpressionStatement(
                    SF.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SF.IdentifierName("_mockBehavior"), 
                        SF.IdentifierName("mockBehavior"))))
                .AddBodyStatements(SF.ReturnStatement(SF.IdentifierName("this")));

            classDclr = classDclr.AddMembers(methodDclr);

            return classDclr;
        }
    }
}