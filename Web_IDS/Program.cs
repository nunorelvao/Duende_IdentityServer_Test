using Duende.IdentityServer;
using Web_IDS.Models;

//get builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();



//inject configuration of middleware into builder

var configIDS = new Config(builder.Configuration);


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

    builder.Services.AddIdentityServer(options =>
        {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;

            options.EmitStaticAudienceClaim = true;

        }).AddTestUsers(TestUsers.Users)
         .AddInMemoryClients(configIDS.Clients)
         .AddInMemoryApiResources(configIDS.ApiResources)
         .AddInMemoryApiScopes(configIDS.ApiScopes)
         .AddInMemoryIdentityResources(configIDS.IdentityResources)
         .AddDeveloperSigningCredential();


//build app
var app = builder.Build();


app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

//add middleware
app.UseIdentityServer();

//Run app
app.Run();
