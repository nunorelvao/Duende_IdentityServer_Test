using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;


namespace Web_IDS.StaticConfig
{
    public class SeedData
    {
        private static ILogger? _logger;
        private static ILoggerFactory _loggerFactory = LoggerFactory.Create(c => c.SetMinimumLevel(LogLevel.Debug));
        public static void EnsureSeedData(WebApplication app)
        {
            _logger = _loggerFactory.CreateLogger("SeedData");

            using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                context.Database.Migrate();
                EnsureSeedData(context);
            }
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                _logger.LogDebug("Clients being populated");
                foreach (var client in StaticConfig.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                _logger.LogDebug("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                _logger.LogDebug("IdentityResources being populated");
                foreach (var resource in StaticConfig.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                _logger.LogDebug("IdentityResources already populated");
            }

            if (!context.ApiScopes.Any())
            {
                _logger.LogDebug("ApiScopes being populated");
                foreach (var resource in StaticConfig.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                _logger.LogDebug("ApiScopes already populated");
            }

            if (!context.ApiResources.Any())
            {
                _logger.LogDebug("ApiResources being populated");
                foreach (var resource in StaticConfig.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                _logger.LogDebug("ApiResources already populated");
            }


            if (!context.IdentityProviders.Any())
            {
                _logger.LogDebug("OIDC IdentityProviders being populated");
                context.IdentityProviders.Add(new OidcProvider
                {
                    Scheme = "demoidsrv",
                    DisplayName = "IdentityServer",
                    Authority = "https://demo.duendesoftware.com",
                    ClientId = "login",
                }.ToEntity());
                context.SaveChanges();
            }
            else
            {
                _logger.LogDebug("OIDC IdentityProviders already populated");
            }
        }
    }
}
