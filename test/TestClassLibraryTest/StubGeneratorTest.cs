using Etg.SimpleStubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClassLibrary;

namespace TestClassLibraryTest
{
    [TestClass]
    public class StubGeneratorTest
    {
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
            testInterface.DoSomething();

            // Switch back to strict mode and check for exception
            stub.MockBehavior = MockBehavior.Strict;
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