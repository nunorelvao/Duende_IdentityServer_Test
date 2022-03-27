using Duende.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Web_IDS.StaticConfig;


var seed = args.Contains("/seed");
if (seed)
{
    args = args.Except(new[] { "/seed" }).ToArray();
}

//get builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();


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

//get connectionString from Config
var connString = builder.Configuration.GetConnectionString("SQLiteConString");
var asseblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

builder.Services.AddIdentityServer(options =>
        {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;

            options.EmitStaticAudienceClaim = true;

        }).AddTestUsers(TestUsers.Users)
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

        .AddDeveloperSigningCredential();


//build app
var app = builder.Build();

if (seed)
{
    Console.WriteLine("Start Seeding Database...");

    Web_IDS.StaticConfig.SeedData.EnsureSeedData(app);

    Console.WriteLine("Done Seeding Database...");

}


app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

//add middleware
app.UseIdentityServer();

//Run app
app.Run();
