
using WeatherMVC.Models;

namespace WeatherMVC.Utils
{
    public static class WeatherDataLinqExtensions
    {

        public static IEnumerable<WeatherDataModel> OnlyLessThan20c(this IEnumerable<WeatherDataModel> source)
        {
            return source.Where(x => x.TemperatureC < 20);
        }
    }
}
