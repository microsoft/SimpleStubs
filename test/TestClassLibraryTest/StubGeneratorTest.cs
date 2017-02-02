using System;
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

        [TestMethod]
        public void TestMockBehaviorProperty_CanBeChangedAtRuntime()
        {
            StubITestInterface stub = new StubITestInterface(MockBehavior.Strict);
            ITestInterface testInterface = stub;
            bool wasExceptionThrownInStrictMode = false;
            bool wasExceptionThrownInStrictMode2ndTime = false;

            // Check for exception in strict mode
            try
            {
                testInterface.DoSomething();
            }
            catch (SimpleStubsException)
            {
                wasExceptionThrownInStrictMode = true;
            }

            // Switch to loose mode, do not expect exception
            stub.MockBehavior = MockBehavior.Loose;

            // SWitch back to strict mode and check for exception
            try
            {
                testInterface.DoSomething();
            }
            catch (SimpleStubsException)
            {
                wasExceptionThrownInStrictMode2ndTime = true;
            }

            Assert.AreEqual(true, wasExceptionThrownInStrictMode);
            Assert.AreEqual(true, wasExceptionThrownInStrictMode2ndTime);
        }
    }
}