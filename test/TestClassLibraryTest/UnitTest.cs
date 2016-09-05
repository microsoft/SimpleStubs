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
        [ExpectedException(typeof(SimpleStubsException))]
        public void TestThatExceptionIsThrownWhenStubIsNotSetup()
        {
            var stub = new StubIPhoneBook();
            IPhoneBook phoneBook = stub;
            phoneBook.GetContactPhoneNumber("John", "Smith");
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
        public void TestGenericMethod()
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
        public void TestOutParameter()
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
        public void TestRefParameter()
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
        public void TestIndexerGet()
        {
            var stub = new StubIGenericContainer<int>();
            stub.Item_Get(index =>
            {
                switch (index)
                {
                    case 0:
                        return 13;
                    case 1:
                        return 5;
                    default:
                        throw new IndexOutOfRangeException();
                }
            });

            IGenericContainer<int> container = stub;
            Assert.AreEqual(13, container[0]);
            Assert.AreEqual(5, container[1]);
        }

        [TestMethod]
        public void TestIndexerSet()
        {
            var stub = new StubIGenericContainer<int>();
            int res = -1;
            stub.Item_Set((index, value) =>
            {
                if (index != 0) throw new IndexOutOfRangeException();
                res = value;
            });

            IGenericContainer<int> container = stub;
            container[0] = 13;

            Assert.AreEqual(13, res);
        }

        [TestMethod]
        public void TestThatMultipleIndexerDontConflict()
        {
            var stub = new StubIGenericContainer<int>();
            stub.Item_Get(index => 12).Item_Get((key, i) => 3);

            IGenericContainer<int> container = stub;
            Assert.AreEqual(12, container[0]);
            Assert.AreEqual(3, container["foo", 0]);
        }

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
    }
}