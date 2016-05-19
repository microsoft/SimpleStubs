using System.Threading.Tasks;

namespace HelloApp
{
    public interface ILocationService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// the location in the format Country/City
        /// </returns>
        /// <exception cref="LocationServiceUnavailableException"></exception>
        Task<string> GetLocation();

        /// <returns>the country code of the given location</returns>
        /// <exception cref="LocationServiceUnavailableException"></exception>
        Task<string> GetCountryCode(string location);
    }
}