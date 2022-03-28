using System.Globalization;
using System.Security.Claims;

namespace WeatherAPI.Extensions
{
    public static class ScopesRequiredHttpContextExtensions
    {
        //
        // Summary:
        //     When applied to an Microsoft.AspNetCore.Http.HttpContext, verifies that the user
        //     authenticated in the web API has any of the accepted scopes. If there is no authenticated
        //     user, the response is a 401 (Unauthenticated). If the authenticated user does
        //     not have any of these acceptedScopes, the method updates the HTTP response providing
        //     a status code 403 (Forbidden) and writes to the response body a message telling
        //     which scopes are expected in the token. We recommend using instead the RequiredScope
        //     Attribute on the controller, the page or the action. See https://aka.ms/ms-id-web/required-scope-attribute.
        //
        // Parameters:
        //   context:
        //     HttpContext (from the controller).
        //
        //   acceptedScopes:
        //     Scopes accepted by this web API.
        public static void VerifyUserHasAnyAcceptedScopeFromJwtToken(this HttpContext context, params string[] acceptedScopes)
        {
            if (acceptedScopes == null)
            {
                throw new ArgumentNullException("acceptedScopes");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ClaimsPrincipal user;
            IEnumerable<Claim> claims;
            lock (context)
            {
                user = context.User;
                claims = user.Claims;
            }

            if (user == null || claims == null || !claims.Any())
            {
                lock (context)
                {
                    context.Response.StatusCode = 401;
                }

                throw new UnauthorizedAccessException("IDW10204: The user is unauthenticated. The HttpContext does not contain any claims. ");
            }

            //first get all scopes claims
            IEnumerable<Claim> claimsFound = user.FindAll("scope");
            if (claimsFound == null)
            {
                claimsFound = user.FindAll("http://schemas.microsoft.com/identity/claims/scope");
            }

            //check all scopes for a valid one
            var claimFound = string.Join(' ', claimsFound).Split(' ').Intersect(acceptedScopes).Any();

            //if not found return error
            if (!claimFound)
            {
                string text = string.Format(CultureInfo.InvariantCulture, "IDW10203: The 'scope' or 'scp' claim does not contain scopes '{0}' or was not found. ", string.Join(",", acceptedScopes));
                context.Response.StatusCode = 403;
                context.Response.WriteAsync(text);
                context.Response.CompleteAsync();
                throw new UnauthorizedAccessException(text);
            }
        }
    }
}

