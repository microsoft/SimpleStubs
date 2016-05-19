using System.Threading.Tasks;
using HelloApp;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HelloAppTest
{
    [TestClass]
    public class LocationManagerTest
    {
        [TestMethod]
        public async Task TestGetCurrentLocation()
        {
            StubILocationService locationServiceStub = new StubILocationService
            {
                ILocationService_GetLocation = () => Task.FromResult("Canada/Vancouver")
            };

            LocationManager locationManager = new LocationManager(locationServiceStub);
            Location location = await locationManager.GetCurrentLocation();

            Assert.AreEqual("Canada", location.Country);
            Assert.AreEqual("Vancouver", location.City);
            Assert.AreEqual(1, locationServiceStub.ILocationService_GetLocation_CallCount);
        }

        [TestMethod]
        public async Task TestThatGetCurrentLocationReturnsNullIfLocationServiceIsUnavailable()
        {
            StubILocationService locationServiceStub = new StubILocationService
            {
                ILocationService_GetLocation = () =>
                {
                    throw new LocationServiceUnavailableException();
                }
            };

            LocationManager locationManager = new LocationManager(locationServiceStub);
            Assert.IsNull(await locationManager.GetCurrentLocation());
            Assert.AreEqual(1, locationServiceStub.ILocationService_GetLocation_CallCount);
        }

        [TestMethod]
        public async Task TestGetCurrentCountryCode()
        {
            StubILocationService locationServiceStub = new StubILocationService
            {
                ILocationService_GetLocation = () => Task.FromResult("Canada/Vancouver"),
                ILocationService_GetCountryCode_String = location => Task.FromResult("CA")
            };

            LocationManager locationManager = new LocationManager(locationServiceStub);
            Assert.AreEqual("CA", await locationManager.GetCurrentCountryCode());
            Assert.AreEqual(1, locationServiceStub.ILocationService_GetCountryCode_String_CallCount);
        }
    }
}
