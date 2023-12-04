
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace LearnNet_ApiGatewayOcelot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            var authConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("ocelot.json");
                    
            });
            builder.ConfigureServices(s =>
            {
                var authenticationProviderKey = "admin_restriction";
                s.AddAuthentication()
                .AddJwtBearer(authenticationProviderKey, options =>
                {
                    options.Authority = authConfiguration["JWT:ValidIssuer"];


                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidAudiences = authConfiguration.GetSection("JWT:ValidAudience").Get<List<string>>();

                    // it's recommended to check the type header to avoid "JWT confusion" attacks
                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                });
                s.AddOcelot();
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                //add your logging
            })
            .UseIISIntegration()
            .Configure(app =>
            {
                app.UseOcelot().Wait();
            });
            
            var app = builder.Build();

            app.Run();
        }
    }
}
