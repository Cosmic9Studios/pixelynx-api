﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AssetStore.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment; 
                    config.AddJsonFile("appsettings.json")
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json");
                })
                .UseKestrel(options => 
                {   
                    options.Listen(IPAddress.Any, 5000, listenOptions => {});
                })
                .UseStartup<Startup>();
    }
}
