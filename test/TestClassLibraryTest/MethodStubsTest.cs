using Etg.SimpleStubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClassLibrary;

namespace TestClassLibraryTest
{
    [TestClass]
    public class MethodStubsTest
    {
        // Test with initialized stubs
        [TestMethod]
        public void TestCallSequence()
        {
            var stub = new StubIPhoneBook()
                .GetContactPhoneNumber((p1, p2) => 12345678, Times.Once) // first call
                .GetContactPhoneNumber((p1, p2) => 11122233, Times.Twice) // next two calls
                .GetContactPhoneNumber((p1, p2) => 22233556, Times.Forever); // rest of the calls

            IPhoneBook phoneBook = stub;
            Assert.AreEqual(12345678, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(11122233, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(11122233, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(22233556, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(22233556, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(22233556, phoneBook.GetContactPhoneNumber("John", "Smith"));
        }

        [TestMethod]
        public void TestMethod_WithReturnType_WithParameters()
        {
            long number = 6041234567;
            string firstName = null;
            string lastName = null;
            var stub = new StubIPhoneBook();
            stub.GetContactPhoneNumber((fn, ln) =>
            {
                firstName = fn;
                lastName = ln;
                return number;
            });
            IPhoneBook phoneBook = stub;
            long actualNumber = phoneBook.GetContactPhoneNumber("John", "Smith");
            Assert.AreEqual(number, actualNumber);
            Assert.AreEqual("John", firstName);
            Assert.AreEqual("Smith", lastName);
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestThatExceptionIsThrownWhenMethodIsCalledMoreThanExpected()
        {
            var stub = new StubIPhoneBook().GetContactPhoneNumber((p1, p2) => 12345678, Times.Once);
            IPhoneBook phoneBook = stub;
            phoneBook.GetContactPhoneNumber("John", "Smith");
            phoneBook.GetContactPhoneNumber("John", "Smith");
        }

        [TestMethod]
        public void TestThatMethodStubCanBeOverwritten()
        {
            var stub = new StubIPhoneBook().GetContactPhoneNumber((p1, p2) => 12345678);
            stub.GetContactPhoneNumber((p1, p2) => 11122233, overwrite: true);

            IPhoneBook phoneBook = stub;
            Assert.AreEqual(11122233, phoneBook.GetContactPhoneNumber("John", "Smith"));
        }

        [TestMethod]
        public void TestGenericMethod_WithReturnType_WithParameter()
        {
            int value = -1;
            var stub = new StubIContainer()
                .GetElement<int>(index => value)
                .SetElement<int>((i, v) => { value = v; });

            IContainer container = stub;
            container.SetElement(0, 5);
            Assert.AreEqual(5, container.GetElement<int>(1));
        }

        [TestMethod]
        public void TestMethod_WithReturnType_IncludesOutParameter()
        {
            object someObj = "test";
            var stub = new StubIContainer()
                .GetElement((int index, out object value) =>
                {
                    value = someObj;
                    return true;
                });

            IContainer container = stub;
            object result;
            container.GetElement(0, out result);
            Assert.AreEqual(someObj, result);
        }

        [TestMethod]
        public void TestMethod_Void_IncludesRefParameters()
        {
            var stub = new StubIRefUtils()
                .Swap<int>((ref int v1, ref int v2) =>
                {
                    int temp = v1;
                    v1 = v2;
                    v2 = temp;
                });

            int i1 = 1;
            int i2 = 2;

            ((IRefUtils)stub).Swap<int>(ref i1, ref i2);
            Assert.AreEqual(2, i1);
            Assert.AreEqual(1, i2);
        }

        [TestMethod]
        public void TestMethod_Void_WithNoParameters()
        {
            var stub = new StubITestInterface();
            bool wasDelegateCalled = false;
            stub.DoSomething(() => { wasDelegateCalled = true; });
            ITestInterface testInterface = stub;
            testInterface.DoSomething();
            Assert.IsTrue(wasDelegateCalled);
        }

        // Test with uninitialized stubs (strict mode)

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestThatExceptionIsThrownWhenStubIsNotSetup()
        {
            var stub = new StubIPhoneBook();
            IPhoneBook phoneBook = stub;
            phoneBook.GetContactPhoneNumber("John", "Smith");
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestMethod_WithReturnType_WithParameters_DefaultBehavior_Strict()
        {
            var stub = new StubIPhoneBook(MockBehavior.Strict);
            IPhoneBook phoneBook = stub;
            Assert.AreEqual(0, phoneBook.GetContactPhoneNumber("John", "Smith"));
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestMethod_Void_WithNoParameters_DefaultBehavior_Strict()
        {
            var stub = new StubITestInterface(MockBehavior.Strict);
            ITestInterface testInterface = stub;
            testInterface.DoSomething();
        }

        // Test with uninitialized stubs (loose mode)

        [TestMethod]
        public void TestMethod_WithReturnType_WithParameters_DefaultBehavior_Loose()
        {
            var stub = new StubIPhoneBook(MockBehavior.Loose);
            IPhoneBook phoneBook = stub;
            Assert.AreEqual(0, phoneBook.GetContactPhoneNumber("John", "Smith"));
        }

        [TestMethod]
        public void TestMethod_Void_WithNoParameters_DefaultBehavior_Loose()
        {
            var stub = new StubITestInterface(MockBehavior.Loose);
            ITestInterface testInterface = stub;
            testInterface.DoSomething();
        }

        [TestMethod]
        public void TestMethod_WithReturnType_IncludesOutParameter_MockBehavior_Loose()
        {
            var stub = new StubIContainer(MockBehavior.Loose);
            IContainer container = stub;
            object outParam;
            Assert.AreEqual(false, container.GetElement(1, out outParam));
            Assert.AreEqual(null, outParam);
        }

        [TestMethod]
        public void TestMethod_WithReturnType_IncludesRefParameter_MockBehavior_Loose()
        {
            var stub = new StubIRefUtils(MockBehavior.Loose);
            IRefUtils refUtils = stub;
            int i1 = 1;
            int i2 = 2;
            refUtils.Swap(ref i1, ref i2);
            Assert.AreEqual(1, i1);
            Assert.AreEqual(2, i2);
        }

        [TestMethod]
        public void TestThatLooseMockBehaviorsAreAlwaysOverwritten()
        {
            var stub = new StubIPhoneBook(MockBehavior.Loose)
                .GetContactPhoneNumber((p1, p2) => 12345678);

            IPhoneBook phoneBook = stub;
            Assert.AreEqual(12345678, phoneBook.GetContactPhoneNumber("John", "Smith"));
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestThatExceptionIsThrownWhenMethodIsCalledMoreThanExpected_DefaultBehavior_Loose()
        {
            var stub = new StubIPhoneBook(MockBehavior.Loose)
                .GetContactPhoneNumber((p1, p2) => 12345678, Times.Once);
            IPhoneBook phoneBook = stub;
            phoneBook.GetContactPhoneNumber("John", "Smith");
            phoneBook.GetContactPhoneNumber("John", "Smith");
        }
    }
}
