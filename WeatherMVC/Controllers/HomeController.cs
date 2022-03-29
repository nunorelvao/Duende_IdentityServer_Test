using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WeatherMVC.Models;
using WeatherMVC.Services;
using WeatherMVC.Utils;

namespace WeatherMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenService _tokenService;

        public HomeController(ILogger<HomeController> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/Home/Index"
            }, "oidc");

        }

        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/Home/Index"
            }, new[] { "oidc", "WeatherMVCCookie" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Weather()
        {

            using var client = new HttpClient();

            ////get token m2m(machine to machine)
            //var tokenm2m = await _tokenService.GetToken("weatherapi.read");
            //client.SetBearerToken(tokenm2m.AccessToken);

            //get token from Pkce method
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            //get from API
            var result = await client.GetAsync("https://localhost:7062/WeatherForecast");

            if (result.IsSuccessStatusCode)
            {
                var model = await result.Content.ReadAsStringAsync();
                                
                var weatherData = model.DeserializeFromJson<List<WeatherDataModel>>(true);

                //use extension method on IeNumerable, just to demonstrate
                var filterWeatherDataLessThan20c = weatherData?.OnlyLessThan20c();

                return View(weatherData);
            }

            throw new Exception("Unable to get content");
        }

        [Authorize]
        public async Task<IActionResult> InsertWeather()
        {

            using var client = new HttpClient();


            //get token from Pkce method
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            List<WeatherDataModel> weatherDataListInput = new List<WeatherDataModel>() {
             new WeatherDataModel()
            {
                Date = DateTime.Now,
                TemperatureC = 20,
                Summary = "MySummary Text"
            }};


            string jsonInString = JsonSerializer.Serialize(weatherDataListInput); 

            //get from API
            var result = await client.PostAsync("https://localhost:7062/WeatherForecast",
                new StringContent(jsonInString, Encoding.UTF8, "application/json"));

            if (result.IsSuccessStatusCode)
            {
                var model = await result.Content.ReadAsStringAsync();

                var weatherData = JsonSerializer.Deserialize<List<WeatherDataModel>>(model);


                return View("Weather", weatherData);
            }

            throw new Exception("Unable to get content");
        }
    }
}