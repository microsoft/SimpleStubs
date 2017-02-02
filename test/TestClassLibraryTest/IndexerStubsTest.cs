using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClassLibrary;

namespace TestClassLibraryTest
{
    [TestClass]
    public class IndexerStubsTest
    {
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
    }
}
