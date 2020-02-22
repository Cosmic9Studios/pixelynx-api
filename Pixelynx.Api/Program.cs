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
using C9S.Configuration.Variables;

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
                    configuration.ResolveVariables("${", "}");
                    var address = configuration.GetSection("Vault:Address").Get<string>();
                    var paths = configuration.GetSection("Vault:Paths").Get<List<string>>();
                    var serviceAccountEmail = configuration.GetSection("GCP:ServiceAccountEmail").Get<string>();
                    var roleName = "";

                    IAuthMethodInfo authMethod = null;
                    if (env.EnvironmentName != "Development") 
                    {
                        roleName = "my-iam-role";
                        authMethod = new GoogleCloudAuthMethodInfo(roleName, 
                            Task.Run(() => GCPHelper.SignJwt(serviceAccountEmail)).Result);
                    }
                    else 
                    {
                        roleName = "admin";
                        authMethod = new TokenAuthMethodInfo("token");
                    }

                    var vaultClientSettings = new VaultClientSettings(address, authMethod);
                    var vaultClient = new VaultClient(vaultClientSettings);
                    var dbCreds = vaultClient.V1.Secrets.Database.GetCredentialsAsync(roleName).Result;
                
                    config.AddHashicorpVault(vaultClient, KVVersion.V1, paths.ToArray());
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"Db:UserName", dbCreds.Data.Username },
                        {"Db:Password", dbCreds.Data.Password }
                    });
                })
                .UseKestrel(options =>
                {   
                    options.Listen(IPAddress.Any, 5000, listenOptions => {});
                    options.Limits.MaxRequestBodySize = null;
                })
                .UseStartup<Startup>();
    }
}