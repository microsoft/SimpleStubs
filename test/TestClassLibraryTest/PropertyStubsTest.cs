using Etg.SimpleStubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClassLibrary;

namespace TestClassLibraryTest
{
    [TestClass]
    public class PropertyStubsTest
    {
        [TestMethod]
        public void TestPropertyStubWithGetterAndSetter()
        {
            long myNumber = 6041234567;
            long newNumber = 0;

            var stub = new StubIPhoneBook()
                .MyNumber_Get(() => myNumber)
                .MyNumber_Set(value => newNumber = value);

            IPhoneBook phoneBook = stub;
            Assert.AreEqual(myNumber, phoneBook.MyNumber);
            phoneBook.MyNumber = 13;
            Assert.AreEqual(13, newNumber);
        }

        [TestMethod]
        public void TestPropertyStubWithGetterOnly()
        {
            int contactsCount = 55;
            var stub = new StubIPhoneBook().ContactsCount_Get(() => contactsCount);
            IPhoneBook phoneBook = stub;
            Assert.AreEqual(contactsCount, phoneBook.ContactsCount);
        }

        [TestMethod]
        public void TestPropertyStubWithGetterAndSetter_Get_MockBehavior_Loose()
        {
            var stub = new StubITestInterface(MockBehavior.Loose);
            ITestInterface testInterface = stub;
            string returnedValue = testInterface.Prop3;
            Assert.IsNull(returnedValue);
        }

        [TestMethod]
        public void TestPropertyStubWithGetterAndSetter_Set_MockBehavior_Loose()
        {
            var stub = new StubITestInterface(MockBehavior.Loose);
            ITestInterface testInterface = stub;
            testInterface.Prop3 = string.Empty;
        }

        [TestMethod]
        public void TestPropertyStubWithGetterOnly_Get_MockBehavior_Loose()
        {
            var stub = new StubITestInterface(MockBehavior.Loose);
            ITestInterface testInterface = stub;
            string returnedValue = testInterface.Prop1;
            Assert.IsNull(returnedValue);
        }

        [TestMethod]
        public void TestPropertyStubWithSetterOnly_Set_MockBehavior_Loose()
        {
            var stub = new StubITestInterface(MockBehavior.Loose);
            ITestInterface testInterface = stub;
            testInterface.Prop2 = string.Empty;
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestPropertyStubWithGetterAndSetter_Get_MockBehavior_Strict()
        {
            var stub = new StubITestInterface(MockBehavior.Strict);
            ITestInterface testInterface = stub;
            string returnedValue = testInterface.Prop3;
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestPropertyStubWithGetterAndSetter_Set_MockBehavior_Strict()
        {
            var stub = new StubITestInterface(MockBehavior.Strict);
            ITestInterface testInterface = stub;
            testInterface.Prop3 = string.Empty;
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestPropertyStubWithGetterOnly_Get_MockBehavior_Strict()
        {
            var stub = new StubITestInterface(MockBehavior.Strict);
            ITestInterface testInterface = stub;
            string returnedValue = testInterface.Prop1;
        }

        [TestMethod]
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestPropertyStubWithSetterOnly_Set_MockBehavior_Strict()
        {
            var stub = new StubITestInterface(MockBehavior.Strict);
            ITestInterface testInterface = stub;
            testInterface.Prop2 = string.Empty;
        }
    }
}