using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen
{
    interface IInterfaceStubber
    {
        CompilationUnitSyntax StubInterface(CompilationUnitSyntax cu, InterfaceDeclarationSyntax interfaceDclr, SemanticModel semanticModel);
    }
}