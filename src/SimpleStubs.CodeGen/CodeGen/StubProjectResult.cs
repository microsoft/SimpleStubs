using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Etg.SimpleStubs.CodeGen.CodeGen
{
    internal class StubProjectResult
    {
        public StubProjectResult(CompilationUnitSyntax cu, IEnumerable<UsingDirectiveSyntax> usings)
        {
            CompilationUnit = cu;
            Usings = usings;
        }

        public CompilationUnitSyntax CompilationUnit { get; }

        public IEnumerable<UsingDirectiveSyntax> Usings { get; }
    }
}