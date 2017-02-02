using System.Threading.Tasks;
using Etg.SimpleStubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClassLibrary;

namespace TestClassLibraryTest
{
    [TestClass]
    public class AsyncMethodStubsTest
    {
        // Test with initialized stubs

        [TestMethod]
        public async Task TestCallSequence_Async()
        {
            var stub = new StubIPhoneBook()
                .GetContactPhoneNumberAsync(async (p1, p2) => await Task.FromResult(12345678), Times.Once) // first call
                .GetContactPhoneNumberAsync(async (p1, p2) => await Task.FromResult(11122233), Times.Twice) // next two calls
                .GetContactPhoneNumberAsync(async (p1, p2) => await Task.FromResult(22233556), Times.Forever); // rest of the calls

            IPhoneBook phoneBook = stub;
            Assert.AreEqual(12345678, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
            Assert.AreEqual(11122233, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
            Assert.AreEqual(11122233, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
            Assert.AreEqual(22233556, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
            Assert.AreEqual(22233556, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
            Assert.AreEqual(22233556, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
        }

        [TestMethod]
        public async Task TestMethod_WithReturnType_WithParameters_Async()
        {
            long number = 6041234567;
            string firstName = null;
            string lastName = null;
            var stub = new StubIPhoneBook();
            stub.GetContactPhoneNumberAsync(async (fn, ln) =>
            {
                firstName = fn;
                lastName = ln;
                return await Task.FromResult(number);
            });
            IPhoneBook phoneBook = stub;
            long actualNumber = await phoneBook.GetContactPhoneNumberAsync("John", "Smith");
            Assert.AreEqual(number, actualNumber);
            Assert.AreEqual("John", firstName);
            Assert.AreEqual("Smith", lastName);
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public async Task TestThatExceptionIsThrownWhenMethodIsCalledMoreThanExpected_Async()
        {
            var stub = new StubIPhoneBook()
                .GetContactPhoneNumberAsync(async (p1, p2) => await Task.FromResult(12345678), Times.Once);
            IPhoneBook phoneBook = stub;
            await phoneBook.GetContactPhoneNumberAsync("John", "Smith");
            await phoneBook.GetContactPhoneNumberAsync("John", "Smith");
        }

        [TestMethod]
        public async Task TestThatMethodStubCanBeOverwritten_Async()
        {
            var stub = new StubIPhoneBook().
                GetContactPhoneNumberAsync(async (p1, p2) => await Task.FromResult(12345678));
            stub.GetContactPhoneNumberAsync(async (p1, p2) => await Task.FromResult(11122233), overwrite: true);

            IPhoneBook phoneBook = stub;
            Assert.AreEqual(11122233, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
        }

        [TestMethod]
        public async Task TestGenericMethod_WithReturnType_WithParameter_Async()
        {
            int value = -1;
            var stub = new StubIContainer()
                .GetElementAsync<int>(async (index) => await Task.FromResult(value))
                .SetElementAsync<int>(async (i, v) => {
                    await Task.Run(() => value = v);
                });

            IContainer container = stub;
            await container.SetElementAsync(0, 5);
            Assert.AreEqual(5, await container.GetElementAsync<int>(1));
        }

        [TestMethod]
        public async Task TestMethod_Void_WithNoParameters_Async()
        {
            var stub = new StubITestInterface();
            bool wasDelegateCalled = false;
            stub.DoSomethingAsync(async () => { await Task.Run(() => wasDelegateCalled = true); });
            ITestInterface testInterface = stub;
            await testInterface.DoSomethingAsync();
            Assert.IsTrue(wasDelegateCalled);
        }

        // Test with uninitialized stubs (strict mode)

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public async Task TestThatExceptionIsThrownWhenStubIsNotSetup_Async()
        {
            var stub = new StubIPhoneBook();
            IPhoneBook phoneBook = stub;
            await phoneBook.GetContactPhoneNumberAsync("John", "Smith");
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public async Task TestMethod_WithReturnType_WithParameters_DefaultBehavior_Strict_Async()
        {
            var stub = new StubIPhoneBook(MockBehavior.Strict);
            IPhoneBook phoneBook = stub;
            Assert.AreEqual(0, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public async Task TestMethod_Void_WithNoParameters_DefaultBehavior_Strict_Async()
        {
            var stub = new StubITestInterface(MockBehavior.Strict);
            ITestInterface testInterface = stub;
            await testInterface.DoSomethingAsync();
        }

        // Test with uninitialized stubs (loose mode)

        [TestMethod]
        public async Task TestMethod_WithReturnType_WithParameters_DefaultBehavior_Loose_Async()
        {
            var stub = new StubIPhoneBook(MockBehavior.Loose);
            IPhoneBook phoneBook = stub;
            Assert.AreEqual(0, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
        }

        [TestMethod]
        public async Task TestMethod_Void_WithNoParameters_DefaultBehavior_Loose_Async()
        {
            var stub = new StubITestInterface(MockBehavior.Loose);
            ITestInterface testInterface = stub;
            await testInterface.DoSomethingAsync();
        }

        [TestMethod]
        public async Task TestThatLooseMockBehaviorsAreAlwaysOverridden_Async()
        {
            var stub = new StubIPhoneBook(MockBehavior.Loose)
                .GetContactPhoneNumberAsync(async (p1, p2) => await Task.FromResult(12345678));

            IPhoneBook phoneBook = stub;
            Assert.AreEqual(12345678, await phoneBook.GetContactPhoneNumberAsync("John", "Smith"));
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public async Task TestThatExceptionIsThrownWhenMethodIsCalledMoreThanExpected_DefaultBehavior_Loose_Async()
        {
            var stub = new StubIPhoneBook(MockBehavior.Loose)
                .GetContactPhoneNumberAsync(async (p1, p2) => await Task.FromResult(12345678), Times.Once);
            IPhoneBook phoneBook = stub;
            await phoneBook.GetContactPhoneNumberAsync("John", "Smith");
            await phoneBook.GetContactPhoneNumberAsync("John", "Smith");
        }
    }
}
