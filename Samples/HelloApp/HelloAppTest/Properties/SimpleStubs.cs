using System.Threading.Tasks;

namespace HelloApp
{
    public class StubILocationService : ILocationService
    {
        global::System.Threading.Tasks.Task<string> global::HelloApp.ILocationService.GetLocation()
        {
            ++ILocationService_GetLocation_CallCount;
            return ILocationService_GetLocation();
        }

        public delegate global::System.Threading.Tasks.Task<string> ILocationService_GetLocation_Delegate();

        public ILocationService_GetLocation_Delegate ILocationService_GetLocation;

        public int ILocationService_GetLocation_CallCount { get; private set; } = 0;

        global::System.Threading.Tasks.Task<string> global::HelloApp.ILocationService.GetCountryCode(string location)
        {
            ++ILocationService_GetCountryCode_String_CallCount;
            return ILocationService_GetCountryCode_String(location);
        }

        public delegate global::System.Threading.Tasks.Task<string> ILocationService_GetCountryCode_String_Delegate(string location);

        public ILocationService_GetCountryCode_String_Delegate ILocationService_GetCountryCode_String;

        public int ILocationService_GetCountryCode_String_CallCount { get; private set; } = 0;
    }
}