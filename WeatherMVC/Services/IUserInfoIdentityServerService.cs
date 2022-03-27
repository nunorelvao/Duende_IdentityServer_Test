using IdentityModel.Client;

namespace WeatherMVC.Services
{
    public interface IUserInfoIdentityServerService
    {
        Task<UserInfoResponse> GetUserInfo(string token);
    }
}