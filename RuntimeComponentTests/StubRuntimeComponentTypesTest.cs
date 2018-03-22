using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using TestRuntimeComponent;
using System;

namespace RuntimeComponentTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task StubbedIAsyncActionIsAwaitable()
        {
            IContainingWinRTSpecificTypes winRtObject = new StubIContainingWinRTSpecificTypes();
            await winRtObject.GetAsyncAction();
        }

        [TestMethod]
        public async Task StubbedIAsyncOperationIsAwaitable()
        {
            IContainingWinRTSpecificTypes winRtObject = new StubIContainingWinRTSpecificTypes();
            bool returnedBool = await winRtObject.GetAsyncOperation();
            Assert.AreEqual(default(bool), returnedBool);
        }
    }
}
