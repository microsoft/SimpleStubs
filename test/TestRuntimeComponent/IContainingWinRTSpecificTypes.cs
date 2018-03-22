using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace TestRuntimeComponent
{
    public interface IContainingWinRTSpecificTypes
    {
        IAsyncAction GetAsyncAction();

        IAsyncOperation<bool> GetAsyncOperation();
    }
}
