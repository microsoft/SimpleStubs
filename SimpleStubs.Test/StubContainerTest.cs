using System;
using Etg.SimpleStubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleStubs.Test
{
    [TestClass]
    public class StubContainerTest
    {
        private delegate void BasicDelegate();
        private delegate void BasicDelegate2();

        private delegate T GenericDelegate<T>();

        [TestMethod]
        public void TestSetGetMethodStub()
        {
            StubContainer<IFoo> container = new StubContainer<IFoo>();
            BasicDelegate del = () => { };
            container.SetMethodStub(del, 1, true);

            Assert.AreEqual(del, container.GetMethodStub<BasicDelegate>("Foo"));
        }

        [TestMethod]
        public void TestThatSetStubOverridesExistingStub()
        {
            StubContainer<IFoo> container = new StubContainer<IFoo>();
            BasicDelegate del = () => { };
            BasicDelegate del2 = () => { throw new Exception();};
            container.SetMethodStub(del, Times.Forever, true);
            container.SetMethodStub(del2, Times.Forever, true);

            Assert.AreEqual(del2, container.GetMethodStub<BasicDelegate>("Foo"));
        }

        [TestMethod]
        public void TestThatDelegatesWithTheSameSignatureDontConflict()
        {
            StubContainer<IFoo> container = new StubContainer<IFoo>();
            BasicDelegate del = () => { };
            BasicDelegate2 del2 = () => { throw new Exception(); };
            container.SetMethodStub(del, Times.Forever, true);
            container.SetMethodStub(del2, Times.Forever, true);

            Assert.AreEqual(del, container.GetMethodStub<BasicDelegate>("Foo1"));
            Assert.AreEqual(del2, container.GetMethodStub<BasicDelegate2>("Foo2"));
        }

        [TestMethod]
        public void TestThatGenericDelegatesWithDifferentGenericTypeDontConflict()
        {
            StubContainer<IFoo> container = new StubContainer<IFoo>();
            GenericDelegate<int> del = () => 1;
            GenericDelegate<string> del2 = () => "text";
            container.SetMethodStub(del, Times.Forever, true);
            container.SetMethodStub(del2, Times.Forever, true);

            Assert.AreEqual(del, container.GetMethodStub<GenericDelegate<int>>("Foo1"));
            Assert.AreEqual(del2, container.GetMethodStub<GenericDelegate<string>>("Foo2"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetMethodStubWhenNotSetup()
        {
            StubContainer<IFoo> container = new StubContainer<IFoo>();
            container.GetMethodStub<BasicDelegate>("Foo");
        }

        private interface IFoo
        {
        };
    }
}
