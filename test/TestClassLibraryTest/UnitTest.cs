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
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethodWithReturnTypeAndParameters()
        {
            long number = 6041234567;
            string firstName = null;
            string lastName = null;
            var stub = new StubIPhoneBook
            {
                GetContactPhoneNumber_String_String = (fn, ln) =>
                {
                    firstName = fn;
                    lastName = ln;
                    return number;
                }
            };
            IPhoneBook phoneBook = stub;
            long actualNumber = phoneBook.GetContactPhoneNumber("John", "Smith");
            Assert.AreEqual(number, actualNumber);
            Assert.AreEqual("John", firstName);
            Assert.AreEqual("Smith", lastName);
        }

        [TestMethod]
        public void TestPropertyStubWithGetterAndSetter()
        {
            long myNumber = 6041234567;
            long newNumber = 0;
            var stub = new StubIPhoneBook
            {
                MyNumber_Get = () => myNumber,
                MyNumber_Set = num =>
                {
                    newNumber = num;
                }
            };
            IPhoneBook phoneBook = stub;
            Assert.AreEqual(myNumber, phoneBook.MyNumber);
            phoneBook.MyNumber = 13;
            Assert.AreEqual(13, newNumber);
        }

        [TestMethod]
        public void TestPropertyStubWithGetterOnly()
        {
            int contactsCount = 55;
            var stub = new StubIPhoneBook { ContactsCount_Get = () => contactsCount };
            IPhoneBook phoneBook = stub;
            Assert.AreEqual(contactsCount, phoneBook.ContactsCount);
        }

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

        [TestMethod]
        public void TestCallSequence()
        {
            var sequence = StubsUtils.Sequence<Func<string, string, int>>()
                .Once((p1, p2) => 12345678) // first call
                .Repeat((p1, p2) => 11122233, 2) // next two call
                .Forever((p1, p2) => 22233556); // rest of the calls
            var stub = new StubIPhoneBook { GetContactPhoneNumber_String_String = (p1, p2) => sequence.Next(p1, p2) };
            IPhoneBook phoneBook = stub;
            Assert.AreEqual(12345678, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(11122233, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(11122233, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(22233556, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(22233556, phoneBook.GetContactPhoneNumber("John", "Smith"));
            Assert.AreEqual(22233556, phoneBook.GetContactPhoneNumber("John", "Smith"));

            Assert.AreEqual(6, sequence.CallCount);
        }

        // this test is only used for debugging
        [Ignore]
        [TestMethod]
        public async Task TestGenerateStubs()
        {
            string path = //@"C:\projects\JasperMain\Product\Jasper.Test\Jasper.Test.csproj";
            @"..\..\TestClassLibraryTest.csproj";
                //"..\\..\\SimpleStubsTest.csproj";
            SimpleStubsGenerator stubsGenerator = new DiModule(path, @"..\..\Properties\SimpleStubs.generated.cs").StubsGenerator;
            string stubs = await stubsGenerator.GenerateStubs(path);
            File.WriteAllText(@"..\..\Properties\SimpleStubs.generated.cs", stubs);
        }
    }
}