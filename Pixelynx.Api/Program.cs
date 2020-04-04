using System;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using VaultSharp.V1.AuthMethods.GoogleCloud;
using Pixelynx.Logic.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using C9S.Configuration.HashicorpVault;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using C9S.Configuration.Variables;
using VaultSharp;

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