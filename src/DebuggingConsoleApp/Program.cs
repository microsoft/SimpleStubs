using System.IO;
using Etg.SimpleStubs.CodeGen;
using Etg.SimpleStubs.CodeGen.DI;

namespace DebuggingConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"..\..\DebuggingConsoleApp.csproj";
            SimpleStubsGenerator stubsGenerator =
                new DiModule(path, @"..\..\Properties\SimpleStubs.generated.cs").StubsGenerator;
            string stubs = stubsGenerator.GenerateStubs(path).Result;
            File.WriteAllText(@"..\..\Properties\SimpleStubs.generated.cs", stubs);
        }
    }
}
