using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen
{
    internal interface IMethodStubber
    {
        ClassDeclarationSyntax StubMethod(ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol,
            INamedTypeSymbol stubbedInterface);
    }
}