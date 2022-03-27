using Microsoft.IdentityModel.Tokens;
using WeatherMVC.Controllers;
using WeatherMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews( config =>
{
    config.Filters.Add(new BaseFilter());
});

//JWTTokenBearer Auth to m2m (machine to machine) api call
builder.Services.Configure<IdentityServerSetttings>(builder.Configuration.GetSection(nameof(IdentityServerSetttings)));

//Authentication with OpenId
builder.Services.AddAuthentication(options =>
   {       
       options.DefaultScheme = "WeatherMVCCookie";
       options.DefaultChallengeScheme = "oidc";
   }).AddCookie("WeatherMVCCookie" , options => options.Cookie.Name = "WeatherMVCCookie")
   .AddOpenIdConnect("oidc", options =>
   {
       options.Authority = builder.Configuration["InteractiveServerSettings:AuthorityUrl"];
       options.ClientId = builder.Configuration["InteractiveServerSettings:ClientId"];
       options.ClientSecret = builder.Configuration["InteractiveServerSettings:ClientSecret"];
       options.Scope.Add(builder.Configuration["InteractiveServerSettings:Scopes:0"]);
       options.Scope.Add(builder.Configuration["InteractiveServerSettings:Scopes:1"]);

       options.ResponseType = "code";
       options.UsePkce = true;
       options.ResponseMode = "query";
       options.SaveTokens = true;
       options.GetClaimsFromUserInfoEndpoint = true;

       options.TokenValidationParameters = new TokenValidationParameters
       {
           NameClaimType = "name",
           RoleClaimType = "role"
       };

   });



builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.Configure<UserInfoIdentityServerSetttings>(builder.Configuration.GetSection(nameof(UserInfoIdentityServerSetttings)));
builder.Services.AddSingleton<IUserInfoIdentityServerService, UserInfoIdentityServerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
