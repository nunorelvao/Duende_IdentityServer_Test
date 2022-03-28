using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
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