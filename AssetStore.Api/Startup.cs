using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetStore.Api.Settings;
using AssetStore.Api.Types;
using AssetStore.Data.BlobStorage;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.GraphiQL;
using HotChocolate.AspNetCore.Playground;
using HotChocolate.AspNetCore.Voyager;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AssetStore.Api
{
    public class Startup
    {
        public IWebHostEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            HostingEnvironment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQL(sp => Schema.Create(c =>
            {
                c.RegisterServiceProvider(sp);
                c.RegisterQueryType<Query>();
            }),
            new QueryExecutionOptions
            {
                TracingPreference = TracingPreference.Always,
                IncludeExceptionDetails = true
            });

            if (HostingEnvironment.EnvironmentName == "Development")
            {
                var blobSettings = new BlobSettings();
                Configuration.GetSection("BlobStorage").Bind(blobSettings);    
    
                services.AddSingleton<IBlobStorage>(new AmazonS3(blobSettings.Address, blobSettings.AccessKey, blobSettings.SecretKey));
            }
            else
            {
                services.AddTransient<IBlobStorage, GCStorage>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (HostingEnvironment.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();
            app.UseGraphQL(new QueryMiddlewareOptions { EnableSubscriptions = false });
            app.UseGraphiQL();
            app.UsePlayground();
            app.UseVoyager();
        }
    }
}
