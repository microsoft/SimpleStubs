using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Etg.SimpleStubs.CodeGen.CodeGen
{
    internal class UsingDirectiveEqualityComparer : IEqualityComparer<UsingDirectiveSyntax>
    {
        public bool Equals(UsingDirectiveSyntax x, UsingDirectiveSyntax y)
        {
            return x.ToString() == y.ToString();
        }

        public int GetHashCode(UsingDirectiveSyntax obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}