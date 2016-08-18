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
	}
}