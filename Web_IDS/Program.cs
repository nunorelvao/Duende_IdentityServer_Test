using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web_IDS;


//get builder
var builder = WebApplication.CreateBuilder(args);

var app = builder
       .ConfigureServices()
       .ConfigurePipeline();


if (args.Contains("/seed"))
{
    Console.WriteLine("Start Seeding Database...");

    Web_IDS.StaticConfig.SeedData.EnsureSeedData(app);

    Console.WriteLine("Done Seeding Database...");

    Console.WriteLine("Start Identity AspNet Seeding Database...");

    Web_IDS.StaticConfig.SeedAspNetUsers.EnsureUsersData(app);

    Console.WriteLine("Done Identity AspNet Seeding Database...");
}


//Run app
app.Run();
