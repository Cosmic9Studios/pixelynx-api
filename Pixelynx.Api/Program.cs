using System;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Pixelynx.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                return settings;
            };

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostEnvironment hostingEnvironment;

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    hostingEnvironment = builderContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json")
                          .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json");
                })
                .UseKestrel(options =>
                {   
                    options.Listen(IPAddress.Any, 5000, listenOptions => {
                        if (hostingEnvironment.EnvironmentName == "Development") {
                            listenOptions.UseHttps("localhost.pfx", "1234");
                        }
                    });
                    options.Limits.MaxRequestBodySize = null;
                })
                .UseStartup<Startup>();
    }
}