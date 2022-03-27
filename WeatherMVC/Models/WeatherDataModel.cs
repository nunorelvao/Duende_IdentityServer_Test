using System.Text.Json.Serialization;

namespace WeatherMVC.Models
{
    public class WeatherDataModel
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }
    }
}
