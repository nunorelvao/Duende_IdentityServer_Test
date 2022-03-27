using IdentityModel.Client;

namespace WeatherMVC.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetToken(string scope);
        [Obsolete]
        Task<TokenResponse> GetTokenWithUserNameAndPassword(string scope, string username, string password);
    }
}