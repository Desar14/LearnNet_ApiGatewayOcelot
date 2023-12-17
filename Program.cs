
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;

namespace LearnNet_ApiGatewayOcelot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("ocelot.json")
                .Build();

            //builder.ConfigureAppConfiguration((hostingContext, config) =>
            //{
            //    config.AddJsonFile("ocelot.json");                    
            //});

            builder.UseConfiguration(configuration);

            builder.ConfigureServices(s =>
            {
                var authenticationProviderKey = "admin_restriction";
                s.AddAuthentication()
                .AddJwtBearer(authenticationProviderKey, options =>
                {
                    options.Authority = configuration["JWT:ValidIssuer"];


                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidAudiences = configuration.GetSection("JWT:ValidAudience").Get<List<string>>();

                    // it's recommended to check the type header to avoid "JWT confusion" attacks
                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                });

                s.AddEndpointsApiExplorer();
                s.AddOcelot()
                    .AddCacheManager(x => x.WithDictionaryHandle());
                s.AddSwaggerGen();
                s.AddSwaggerForOcelot(configuration,
                  (o) =>
                  {
                      o.GenerateDocsForAggregates = true;
                  });

            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                //add your logging
            })
            .UseIISIntegration()
            .Configure(app =>
            {                
                app.UseSwaggerForOcelotUI(opt =>
                {
                    opt.PathToSwaggerGenerator = "/swagger/docs";
                }, 
                uiOpt => {
                    uiOpt.DocumentTitle = "Gateway documentation";
                });

                app.UseOcelot().Wait();
            });
            
            var app = builder.Build();

            app.Run();
        }
    }
}
