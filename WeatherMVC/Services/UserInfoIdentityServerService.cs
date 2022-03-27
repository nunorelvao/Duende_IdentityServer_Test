using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace WeatherMVC.Services
{


    public class UserInfoIdentityServerService : IUserInfoIdentityServerService
    {
        private readonly ILogger<UserInfoIdentityServerService> _logger;
        private readonly IOptions<UserInfoIdentityServerSetttings> _identityServerUserSetttings;

        public UserInfoIdentityServerService(ILogger<UserInfoIdentityServerService> logger, IOptions<UserInfoIdentityServerSetttings> identityServerUserSetttings)
        {
            _logger = logger;
            _identityServerUserSetttings = identityServerUserSetttings;

        }

        public async Task<UserInfoResponse> GetUserInfo(string token)
        {
            using var httpClient = new HttpClient();


            var response = await httpClient.GetUserInfoAsync(new UserInfoRequest
            {
                Address = _identityServerUserSetttings.Value.UserInfoUrl,
                Token = token
            });

            if (response.IsError)
            {
                _logger.LogError($"Unable to get User Info Error is: {response.Error}");
                throw new Exception("unable to get User Info", response.Exception);
            }

            return response;
        }
    }
}
