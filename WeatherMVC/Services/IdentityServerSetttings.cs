namespace WeatherMVC.Services
{
    public class IdentityServerSetttings
    {
        public string? DiscoveryUrl { get; set; }
        public string? ClientName { get; set; }
        public string? ClientPassword { get; set; }
        public bool UseHtps { get; set; }
    }
}
