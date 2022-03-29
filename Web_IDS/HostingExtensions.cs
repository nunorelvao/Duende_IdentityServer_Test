using Duende.IdentityServer;
using IdentityServerHost.Pages.Admin.ApiScopes;
using IdentityServerHost.Pages.Admin.Clients;
using IdentityServerHost.Pages.Admin.IdentityScopes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web_IDS.Data;
using Web_IDS.Models;

namespace Web_IDS;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {

        //get connectionString from Config
        var connString = builder.Configuration.GetConnectionString("SQLiteConString");
        var asseblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        builder.Services.AddRazorPages();


        //Add DbContext for Identity
        var aspNetCoreIdentityconnString = builder.Configuration.GetConnectionString("SQLiteIdentityAspNetCoreConString");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(aspNetCoreIdentityconnString, options => options.MigrationsAssembly(asseblyName));
        });
                

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;

                options.Caching.ClientStoreExpiration = TimeSpan.FromMinutes(5);
                options.Caching.ResourceStoreExpiration = TimeSpan.FromMinutes(5);
            })
            //.AddInMemoryClients(StaticConfig.Clients) //This are in memory Stores to test and used in seed initial data
            //.AddInMemoryApiResources(StaticConfig.ApiResources) //This are in memory Stores to test and used in seed initial data
            //.AddInMemoryApiScopes(StaticConfig.ApiScopes) //This are in memory Stores to test and used in seed initial data
            //.AddInMemoryIdentityResources(StaticConfig.IdentityResources) //This are in memory Stores to test and used in seed initial data
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlite(connString, options => options.MigrationsAssembly(asseblyName));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlite(connString, options => options.MigrationsAssembly(asseblyName));
            })
            .AddInMemoryCaching() //Currently uses InMemoryCache , but desireable in production should use eg NCache or RedisCache (https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-6.0)
            .AddConfigurationStoreCache() // this is a cache you will want in production to reduce load on and requests to the DB
            .AddAspNetIdentity<ApplicationUser>();


        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = builder.Configuration["google.clientid"];
                options.ClientSecret = builder.Configuration["google.clientsecret"];
            })
            .AddTwitter(options =>
            {
                ////To be used with Oauth1 package "Microsoft.AspNetCore.Authentication.Twitter"
                //options.ConsumerKey = builder.Configuration["twitter.consumerkey"];
                //options.ConsumerSecret = builder.Configuration["twitter.consumersecret"];

                //To be used with Oauth2 package 

                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = builder.Configuration["twitter.clientid"];
                options.ClientSecret = builder.Configuration["twitter.clientsecret"];

            })
           .AddMicrosoftAccount(options =>
           {
               options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
               options.ClientId = builder.Configuration["microsoft.clientid"];
               options.ClientSecret = builder.Configuration["microsoft.clientsecret"];
           })
          .AddGitHub(options =>
          {
              options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
              options.ClientId = builder.Configuration["github.clientid"];
              options.ClientSecret = builder.Configuration["github.clientsecret"];
              //options.EnterpriseDomain = builder.Configuration["github.enterprisedomain"];
              options.Scope.Add("user:email");
          });

        // this adds the necessary config for the simple admin/config pages
        {
            builder.Services.AddAuthorization(options =>
                options.AddPolicy("admin",
                    policy => policy.RequireClaim("sub", "1"))
            );

            builder.Services.Configure<RazorPagesOptions>(options =>
                options.Conventions.AuthorizeFolder("/Admin", "admin"));

            builder.Services.AddTransient<ClientRepository>();
            builder.Services.AddTransient<IdentityScopeRepository>();
            builder.Services.AddTransient<ApiScopeRepository>();
        }


        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
      
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}