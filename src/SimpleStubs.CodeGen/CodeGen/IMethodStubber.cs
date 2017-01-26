using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen
{
    internal interface IMethodStubber
    {
        ClassDeclarationSyntax StubMethod(CompilationUnitSyntax cu, ClassDeclarationSyntax classDclr, IMethodSymbol methodSymbol, INamedTypeSymbol stubbedInterface, SemanticModel semanticModel);
    }
}