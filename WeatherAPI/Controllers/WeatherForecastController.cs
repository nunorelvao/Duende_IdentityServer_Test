using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Text.Json;
using WeatherAPI.Extensions;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        const string apiReadScope = "weatherapi.read";
        const string apiWriteScope = "weatherapi.write";
        IAuthorizationService _authz;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAuthorizationService authz)
        {
            _logger = logger;
            _authz = authz;
        }
        
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            //using the policy set in authorizations although this returns a Http 500 by default  
            await _authz.AuthorizeAsync(User, "apiread");

            //kind of model validate but on scopes vaidate with extension (you can easly define return http code in extension)
            HttpContext.VerifyUserHasAnyAcceptedScopeFromJwtToken(apiReadScope);

            var res = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .AsEnumerable<WeatherForecast>();
                      
            
            return res;

        }

        [HttpPost(Name = "InsertWeather")]
        public async Task<IActionResult> InsertWeatherAsync(IEnumerable<WeatherForecast> model)
        {
            //using the policy set in authorizations although this returns a Http 500 by default  
            await _authz.AuthorizeAsync(User, "apiwrite");

            //kind of model validate but on scopes vaidate with extension (you can easly define return http code in extension)
            HttpContext.VerifyUserHasAnyAcceptedScopeFromJwtToken(apiWriteScope);

            var sermodel = JsonSerializer.Serialize<IEnumerable<WeatherForecast>>(model);

            return Ok(sermodel);
        }
    }
}