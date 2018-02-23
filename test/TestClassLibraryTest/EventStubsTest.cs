using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClassLibrary;

namespace TestClassLibraryTest
{
    [TestClass]
    public class EventStubsTest
    {
        [TestMethod]
        public void TestEventStub()
        {
            object sender = null;
            long newNumber = 0;
            var stub = new StubIPhoneBook();
            stub.PhoneNumberChanged += (s, num) =>
            {
                sender = s;
                newNumber = num;
            };

            stub.PhoneNumberChanged_Raise(this, 55);
            Assert.AreEqual(55, newNumber);
            Assert.AreEqual(this, sender);
        }

        [TestMethod]
        public void TestCustomDelegateBasedEventStub()
        {
            int arg1 = 0;
            string arg2 = null;
            object arg3 = null;
            var stub = new StubICustomDelegateBasedEventExample();
            stub.CustomDelegateEventOccurred += (i, s, o) =>
            {
                arg1 = i;
                arg2 = s;
                arg3 = o;
            };

            stub.CustomDelegateEventOccurred_Raise(55, "test", new Random(1));

            Assert.AreEqual(55, arg1);
            Assert.AreEqual("test", arg2);
            Assert.AreEqual(typeof(Random), arg3.GetType());
        }

        [TestMethod]
        public void TestCustomDelegateBasedWithReturnTypeEventStub()
        {
            var stub = new StubICustomDelegateBasedEventExample();
            stub.CustomDelegateWithReturnTypeEventOccurred += (sender, args) => "Teststring1";
            stub.CustomDelegateWithReturnTypeEventOccurred += (sender, args) => "Teststring2";
            stub.CustomDelegateWithReturnTypeEventOccurred += (sender, args) => "Teststring3";

            var receivedList = stub.CustomDelegateWithReturnTypeEventOccurred_Raise(stub, EventArgs.Empty).ToList();

            Assert.AreEqual(receivedList.Count, 3);
            Assert.AreEqual(receivedList[0], "Teststring1");
            Assert.AreEqual(receivedList[1], "Teststring2");
            Assert.AreEqual(receivedList[2], "Teststring3");
        }

        [TestMethod]
        public void TestCustomDelegateBasedWithReturnTypeWithParameterEventStub()
        {
            var stub = new StubICustomDelegateBasedEventExample();
            stub.CustomDelegateWithReturnTypeWithParameterEventOccurred += args => args + "Teststring1";
            stub.CustomDelegateWithReturnTypeWithParameterEventOccurred += args => args + "Teststring2";
            stub.CustomDelegateWithReturnTypeWithParameterEventOccurred += args => args + "Teststring3";

            var receivedList = stub.CustomDelegateWithReturnTypeWithParameterEventOccurred_Raise("My").ToList();

            Assert.AreEqual(receivedList.Count, 3);
            Assert.AreEqual(receivedList[0], "MyTeststring1");
            Assert.AreEqual(receivedList[1], "MyTeststring2");
            Assert.AreEqual(receivedList[2], "MyTeststring3");
        }
    }
}
