using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using Web_IDS.Data;
using Web_IDS.Models;

namespace Web_IDS.StaticConfig
{
    public class SeedAspNetUsers
    {
        public static void EnsureUsersData(WebApplication app)
        {
           var logger = app.Services.GetRequiredService<ILogger<SeedAspNetUsers>>();

            using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {                               
               
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context?.Database.Migrate();
                               
                EnsureUsersData(context, scope, logger);
            }
        }

        private static void EnsureUsersData(ApplicationDbContext? context, IServiceScope scope, ILogger? logger)
        {


            if (context != null && !context.Users.Any())
            {
                logger?.LogDebug("Users being populated");
                foreach (var user in TestUsers.Users.ToList())
                {
                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var userToCreate = userMgr.FindByNameAsync(user.Username).Result;

                    if (userToCreate == null)
                    {
                        userToCreate = new ApplicationUser
                        {
                            UserName = user.Username,
                            Email = user.Claims?.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value,
                            EmailConfirmed = true
                        };

                        var result = userMgr.CreateAsync(userToCreate, user.Password).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        var address = JsonSerializer.Serialize(new
                        {
                            street_address = "One Hacker Way",
                            locality = "Heidelberg",
                            postal_code = 69118,
                            country = "Germany"
                        });


                        result = userMgr.AddClaimsAsync(userToCreate, new List<Claim>()
                        {
                            new Claim(JwtClaimTypes.Role, user.Claims?.FirstOrDefault(c => c.Type == JwtClaimTypes.Role)?.Value ?? ""),
                            new Claim(JwtClaimTypes.Name, user.Claims?.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value ?? ""),
                            new Claim(JwtClaimTypes.GivenName, user.Claims?.FirstOrDefault(c => c.Type == JwtClaimTypes.GivenName)?.Value ?? ""),
                            new Claim(JwtClaimTypes.FamilyName, user.Claims?.FirstOrDefault(c => c.Type == JwtClaimTypes.FamilyName)?.Value ?? ""),
                            new Claim(JwtClaimTypes.Email, user.Claims?.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value ?? ""),
                            new Claim(JwtClaimTypes.EmailVerified, "true", System.Security.Claims.ClaimValueTypes.Boolean),
                            new Claim(JwtClaimTypes.WebSite, user.Claims?.FirstOrDefault(c => c.Type == JwtClaimTypes.WebSite)?.Value ?? ""),
                            new System.Security.Claims.Claim(JwtClaimTypes.Address, address, IdentityServerConstants.ClaimValueTypes.Json)
                        }).Result;
                    }

                    
                }
                context.SaveChanges();
            }
            else
            {
                logger?.LogDebug("Users already populated");
            }

        }
    }
}
