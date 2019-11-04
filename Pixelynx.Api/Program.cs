using System;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods.GoogleCloud;
using Pixelynx.Logic.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using C9S.Configuration.HashicorpVault;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment; 
                    config.AddJsonFile("appsettings.json")
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json");
                    
                    var configuration = config.Build();
                    var address = configuration.GetSection("Vault:Address").Get<string>();
                    var paths = configuration.GetSection("Vault:Paths").Get<List<string>>();

                    IAuthMethodInfo authMethod = null;
                    if (env.EnvironmentName == "Production") 
                    {
                        authMethod = new GoogleCloudAuthMethodInfo("my-iam-role", 
                            Task.Run(() => GCPHelper.SignJwt("assetstore", "lynxbot@pixelynx.iam.gserviceaccount.com")).Result);
                    }
                    else 
                    {
                        authMethod = new TokenAuthMethodInfo("token");
                    }

                    var vaultClientSettings = new VaultClientSettings(address, authMethod);
                    var vaultClient = new VaultClient(vaultClientSettings);
                
                    config.AddHashicorpVault(vaultClient, KVVersion.V1, paths.ToArray());
                })
                .UseKestrel(options =>
                {   
                    options.Listen(IPAddress.Any, 5000, listenOptions => {});
                })
                .UseStartup<Startup>();
    }
}