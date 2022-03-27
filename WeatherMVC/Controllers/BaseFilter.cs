using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WeatherMVC.Controllers
{
    public class BaseFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User != null)
            {
                var ident = context.HttpContext.User.Identity;
                if (ident != null && ident.IsAuthenticated)
                {
                    var valtypeFound = context.HttpContext.User.Claims.FirstOrDefault(f => f.Type == "role");
                    if (valtypeFound != null) {
                        string hasPermission = valtypeFound.Value;
                        if (hasPermission == "user")
                        {
                            context.Result = new ForbidResult();
                        }
                    }
                }                
            }
            else
            {
                context.Result = new RedirectResult("/Home/Index");
            }
        }
    }
}
