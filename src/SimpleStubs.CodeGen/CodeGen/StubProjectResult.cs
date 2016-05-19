using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Etg.SimpleStubs.CodeGen.CodeGen
{
    class StubProjectResult
    {
        public StubProjectResult(CompilationUnitSyntax cu, IEnumerable<string> usings)
        {
            CompilationUnit = cu;
            Usings = usings;
        }

        public CompilationUnitSyntax CompilationUnit
        {
            get;
        }

        public IEnumerable<string> Usings
        {
            get;
        }
    }
}