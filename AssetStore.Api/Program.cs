using System;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AssetStore.Api.Helpers;

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
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                          .AddJsonFile(new EncryptedFileProvider(), "appsecrets.json.encrypted", false, true);
                })
                .UseKestrel(options => 
                {   
                    options.Listen(IPAddress.Any, 5000, listenOptions => {});
                })
                .UseStartup<Startup>();
    }
}
