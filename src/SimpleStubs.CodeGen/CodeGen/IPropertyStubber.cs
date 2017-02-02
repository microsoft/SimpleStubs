using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen
{
    internal interface IPropertyStubber
    {
        ClassDeclarationSyntax StubProperty(ClassDeclarationSyntax classDclr, IPropertySymbol propertySymbol, INamedTypeSymbol stubbedInterface, SemanticModel semanticModel);
    }
}