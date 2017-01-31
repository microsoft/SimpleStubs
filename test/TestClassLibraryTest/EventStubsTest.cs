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
            }
                ;
            stub.PhoneNumberChanged_Raise(this, 55);
            Assert.AreEqual(55, newNumber);
            Assert.AreEqual(this, sender);
        }
    }
}
