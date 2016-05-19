using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.PointOfService;

namespace HelloApp
{
    public class LocationManager
    {
        private readonly ILocationService _locationService;
        public LocationManager(ILocationService locationService)
        {
            _locationService = locationService;
        }

        /// <returns>Current Location or null if the location could not be retrieved</returns>
        public async Task<Location> GetCurrentLocation()
        {
            try
            {
                string location = await _locationService.GetLocation();
                var ss = location.Split('/');
                return new Location(ss[0], ss[1]);
            }
            catch (LocationServiceUnavailableException)
            {
                return null;
            }
        }

        /// <returns>The current country code (e.g. US, CA) or null if the country code could not be retrieved</returns>
        public async Task<string> GetCurrentCountryCode()
        {
            try
            {
                Location location = await GetCurrentLocation();
                string loc = $"{location.Country}/{location.City}";
                return await _locationService.GetCountryCode(loc);
            }
            catch (LocationServiceUnavailableException)
            {
                return null;
            }
        }
    }
}
