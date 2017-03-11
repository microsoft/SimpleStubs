using System;
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
    }
}
