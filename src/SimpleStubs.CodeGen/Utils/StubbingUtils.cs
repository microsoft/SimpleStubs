namespace Etg.SimpleStubs.CodeGen.Utils
{
    internal class StubbingUtils
    {
        public static string GenerateInvokeDelegateStmt(string delegateTypeName, string parameters)
        {
            return $"(({delegateTypeName})_stubs[nameof({delegateTypeName})]).Invoke({parameters});\n";
        }
    }
}