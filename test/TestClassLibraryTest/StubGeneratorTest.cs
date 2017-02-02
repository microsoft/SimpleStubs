using System.IO;
using System.Threading.Tasks;
using Etg.SimpleStubs;
using Etg.SimpleStubs.CodeGen;
using Etg.SimpleStubs.CodeGen.DI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClassLibrary;

namespace TestClassLibraryTest
{
    [TestClass]
    public class StubGeneratorTest
    {
        // this test is only used for debugging
        [Ignore]
        [TestMethod]
        public async Task TestGenerateStubs()
        {
            string path = //@"C:\projects\JasperMain\Product\Jasper.Test\Jasper.Test.csproj";
                @"..\..\TestClassLibraryTest.csproj";
            //"..\\..\\SimpleStubsTest.csproj";
            SimpleStubsGenerator stubsGenerator =
                new DiModule(path, @"..\..\Properties\SimpleStubs.generated.cs").StubsGenerator;
            string stubs = await stubsGenerator.GenerateStubs(path);
            File.WriteAllText(@"..\..\Properties\SimpleStubs.generated.cs", stubs);
        }

        [TestMethod]
        public void TestMockBehaviorProperty_MatchesConstructorSuppliedValue_Loose()
        {
            StubITestInterface stub = new StubITestInterface(MockBehavior.Loose);
            Assert.AreEqual(MockBehavior.Loose, stub.MockBehavior);
        }

        [TestMethod]
        public void TestMockBehaviorProperty_MatchesConstructorSuppliedValue_Strict()
        {
            StubITestInterface stub = new StubITestInterface(MockBehavior.Strict);
            Assert.AreEqual(MockBehavior.Strict, stub.MockBehavior);
        }
    }
}