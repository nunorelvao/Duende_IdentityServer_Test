using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using Web_IDS.Models;

namespace DuendeIdentityServer.Pages.ExternalLogin;

[AllowAnonymous]
[SecurityHeaders]
public class Callback : PageModel
{
    //private readonly TestUserStore _users;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<Callback> _logger;
    private readonly IEventService _events;
    //add because of Identity
    private readonly SignInManager<ApplicationUser> _signInManager;

    public Callback(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Callback> logger,
         /*TestUserStore users = null*/
         SignInManager<ApplicationUser> signInManager)
    {
        // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
        //_users = users ?? throw new Exception("Please call 'AddTestUsers(TestUsers.Users)' on the IIdentityServerBuilder in Startup or remove the TestUserStore from the AccountController.");

        _interaction = interaction;
        _logger = logger;
        _events = events;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet()
    {
        // read external identity from the temporary cookie
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }

        var externalUser = result.Principal;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = externalUser?.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.LogDebug("External claims: {@claims}", externalClaims);
        }

        // lookup our user and external provider info
        // try to determine the unique id of the external user (issued by the provider)
        // the most common claim type for that are the sub claim and the NameIdentifier
        // depending on the external provider, some other claim type might be used
        var userIdClaim = externalUser?.FindFirst(JwtClaimTypes.Subject) ??
                          externalUser?.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new Exception("Unknown userid");

        var provider = result.Properties?.Items["scheme"];
        var providerUserId = userIdClaim.Value;


        var user = await _signInManager.UserManager.FindByLoginAsync(provider, providerUserId);
        // find external user
        //var user = _users.FindByExternalProvider(provider, providerUserId);

        if (user == null)
        {
            // this might be where you might initiate a custom workflow for user registration
            // in this sample we don't show how that would be done, as our sample implementation
            // simply auto-provisions new external user
            //
            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var userName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == JwtClaimTypes.Name)?.Value;

            if (userName == null)
            {
                userName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid || c.Type == JwtClaimTypes.SessionId)?.Value;
            }

            //nromalize  susername
            userName = userName?.Normalize(NormalizationForm.FormD);
            var chars = userName?.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            userName =  new string(chars).Normalize(NormalizationForm.FormC);
            userName = userName?.Trim().Replace(" ", "").ToLower();
            userName = userName + "-" + provider?.ToLower();

            var normUserName = userName?.ToUpper();

            var userEmail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == JwtClaimTypes.Email)?.Value;
            var normUserEmail = userEmail?.ToUpper();


            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName,
                NormalizedUserName = normUserName,
                Email = userEmail
            };

            //create new user and add to store
            var created = await _signInManager.UserManager.CreateAsync(newUser);

            if (!created.Succeeded)
            {
                _logger.LogError($"External User {newUser.UserName} could not be created Error: {created.Errors?.First()?.Description}", newUser);
            }

            //add all claims to store
            await _signInManager.UserManager.AddClaimsAsync(newUser, claims);

            //add login to store
            var loginAdded = await _signInManager.UserManager.AddLoginAsync(newUser, new UserLoginInfo(provider, providerUserId, provider));

            if (!loginAdded.Succeeded)
            {
                _logger.LogError($"Login for User {newUser.UserName} could not be added Error: {loginAdded.Errors?.First()?.Description}", newUser);
            }

            user = newUser;
        }

        // this allows us to collect any additional claims or properties
        // for the specific protocols used and store them in the local auth cookie.
        // this is typically used to store data needed for signout from those protocols.
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        CaptureExternalLoginContext(result, additionalLocalClaims, localSignInProps);

        // issue authentication cookie for user
        var isuser = new IdentityServerUser(user.Id)
        {
            DisplayName = user.UserName,
            IdentityProvider = provider,
            AdditionalClaims = additionalLocalClaims
        };

        await HttpContext.SignInAsync(isuser, localSignInProps);

        // delete temporary cookie used during external authentication
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // retrieve return URL
        var returnUrl = result.Properties?.Items["returnUrl"] ?? "~/";

        // check if external login is in the context of an OIDC request
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, user.UserName, true, context?.Client.ClientId));

        if (context != null)
        {
            if (context.IsNativeClient())
            {
                // The client is native, so this change in how to
                // return the response is for better UX for the end user.
                return this.LoadingPage(returnUrl);
            }
        }

        return Redirect(returnUrl);
    }

    // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
    // this will be different for WS-Fed, SAML2p or other protocols
    private void CaptureExternalLoginContext(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        // if the external system sent a session id claim, copy it over
        // so we can use it for single sign-out
        var sid = externalResult?.Principal?.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // if the external provider issued an id_token, we'll keep it for signout
        var idToken = externalResult?.Properties?.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
        }
    }
}