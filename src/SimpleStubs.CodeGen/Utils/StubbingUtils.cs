using System.Linq;
using Microsoft.CodeAnalysis;

namespace Etg.SimpleStubs.CodeGen.Utils
{
    internal class StubbingUtils
    {
        public static string GenerateInvokeDelegateStmt(string delegateTypeName, string methodName, string parameters)
        {
            return $"_stubs.GetMethodStub<{delegateTypeName}>(\"{methodName}\").Invoke({parameters});\n";
        }

        public static string FormatParameters(IMethodSymbol methodSymbol)
        {
            return string.Join(", ", methodSymbol.Parameters.Select(p =>
            {
                if (p.RefKind == RefKind.Out)
                {
                    return $"out {p.Name}";
                }
                if (p.RefKind == RefKind.Ref)
                {
                    return $"ref {p.Name}";
                }
                return p.Name;
            }));
        }
    }
}