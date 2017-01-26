using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen
{
    internal interface IFluentConfigurationStubber
    {
        ClassDeclarationSyntax StubMockBehaviorConfigurationMethod(INamedTypeSymbol classDclr, ClassDeclarationSyntax nonGenericStubName);
    }
}