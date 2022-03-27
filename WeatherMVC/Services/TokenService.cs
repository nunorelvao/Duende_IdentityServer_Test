using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace WeatherMVC.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly IOptions<IdentityServerSetttings> _identityServerSetttings;
        private readonly DiscoveryDocumentResponse _discoveryDocumentResponse;

        public TokenService(ILogger<TokenService> logger, IOptions<IdentityServerSetttings> identityServerSetttings)
        {
            _logger = logger;
            _identityServerSetttings = identityServerSetttings;

            using var httpClient = new HttpClient();
            _discoveryDocumentResponse =
                httpClient.GetDiscoveryDocumentAsync(identityServerSetttings.Value.DiscoveryUrl).Result;

            if (_discoveryDocumentResponse.IsError)
            {
                _logger.LogError($"Unable to get discovery document. Error is : {_discoveryDocumentResponse.Error}");
                throw new Exception("Unable to get discovery document", _discoveryDocumentResponse.Exception);
            }
        }
        public async Task<TokenResponse> GetToken(string scope)
        {
            using var httpClient = new HttpClient();

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = _discoveryDocumentResponse.TokenEndpoint,
                ClientId = _identityServerSetttings.Value.ClientName,
                ClientSecret = _identityServerSetttings.Value.ClientPassword,
                Scope = scope
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError($"Unable to get Token. Error is: {tokenResponse.Error}");
                throw new Exception("unable to get token", tokenResponse.Exception);
            }

            return tokenResponse;
        }

        [Obsolete] //removed flow in Oauth 2.1
        public async Task<TokenResponse> GetTokenWithUserNameAndPassword(string scope, string username, string password)
        {
            using var httpClient = new HttpClient();

            var tokenResponse = await httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = _discoveryDocumentResponse.TokenEndpoint,
                ClientId = _identityServerSetttings.Value.ClientName,
                ClientSecret = _identityServerSetttings.Value.ClientPassword,
                Scope = scope,

                UserName = username,
                Password = password
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError($"Unable to get Token with Username and Password. Error is: {tokenResponse.Error}");
                throw new Exception("unable to get token with username and password", tokenResponse.Exception);
            }

            return tokenResponse;
        }

    }
}
 