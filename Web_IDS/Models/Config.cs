using Duende.IdentityServer.Models;

namespace Web_IDS.Models
{
    public class Config
    {
        private readonly IConfiguration _config;
        private readonly string _m2mclientId;

        public Config(IConfiguration config)
        {
            _config = config;

            _m2mclientId = _config["m2mclientID"]; ///get value from secret (not so secret :) just to highlight 12050f74-2385-4d9f-8f50-4b75e2c0cf0e)
        }

        public IEnumerable<IdentityResource> IdentityResources => new[]
          {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new IdentityResources.Address(),
            new IdentityResource
            {
              Name = "identityresources",
              //UserClaims = new List<string> {"sub",
              //                              "name",
              //                              "family_name",
              //                              "given_name",
              //                              "middle_name",
              //                              "nickname",
              //                              "preferred_username",
              //                              "profile",
              //                              "picture",
              //                              "website",
              //                              "gender",
              //                              "birthdate",
              //                              "zoneinfo",
              //                              "locale",
              //                              "updated_at",
              //                              "role",
              //                              "email"}

              UserClaims = new List<string> {"role", "email"}
            }
          };

        public IEnumerable<ApiScope> ApiScopes => new[]
        {
            new ApiScope("weatherapi.read"),
            new ApiScope("weatherapi.write"),
            new ApiScope("api1.read"),
            new ApiScope("api1.write"),
        };

        public IEnumerable<ApiResource> ApiResources => new[]
        {
          new ApiResource("weatherapi")
          {
            Scopes = new List<string> {"weatherapi.read", "weatherapi.write"},
            ApiSecrets = new List<Secret> {new Secret("9ffca48f-a400-47ea-93b9-78ac564d2b28".Sha256())},
            UserClaims = new List<string> {"role"}
          },
          new ApiResource("api1")
          {
            Scopes = new List<string> {"api1.read", "api1.write"},
            ApiSecrets = new List<Secret> {new Secret("31e1f3e1-7420-4ffc-8f40-a15d286eabc4".Sha256())},
            UserClaims = new List<string> {"role"}
          }
        };


        public IEnumerable<Client> Clients => new[]
          {

            // m2m client credentials flow client (machine 2 machine eg:API)
            new Client
            {
              ClientId = "m2m.client",
              ClientName = "Client Credentials Client",

              AllowedGrantTypes =  GrantTypes.ClientCredentials,
              ClientSecrets = {new Secret(_m2mclientId.Sha256())},

              AllowedScopes = {"weatherapi.read", "weatherapi.write"}
            },


            // interactive client using code flow + pkce (eg browser redirect)
            new Client
            {
              ClientId = "interactive",
              ClientSecrets = {new Secret("17cabcc7-266a-4e31-9bae-b83101f35c27".Sha256())},

              AllowedGrantTypes = GrantTypes.Code,

              RedirectUris = {"https://localhost:7262/signin-oidc"},
              FrontChannelLogoutUri = "https://localhost:7262/signout-oidc",
              PostLogoutRedirectUris = {"https://localhost:7262/signout-callback-oidc"},

              AllowOfflineAccess = true,
              AllowedScopes = {"openid", "profile", "identityresources", "weatherapi.read"},
              RequirePkce = true,
              //RequireConsent = true,
              AllowPlainTextPkce = false,
              UserSsoLifetime = 15

            },
        };
    }
}

