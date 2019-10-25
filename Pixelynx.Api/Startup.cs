using Pixelynx.Api.Settings;
using Pixelynx.Api.Types;
using Pixelynx.Data.BlobStorage;
using C9S.Configuration.Variables;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Voyager;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pixelynx.Api
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
            services.AddLogging(configure => configure.AddConsole());
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

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

            Configuration.ResolveVariables("${", "}");
            if (HostingEnvironment.EnvironmentName == "Development")
            {
                var blobSettings = new BlobSettings();
                Configuration.GetSection("BlobStorage").Bind(blobSettings);    
    
                services.AddSingleton<IBlobStorage>(new AmazonS3(blobSettings.Address, blobSettings.AccessKey, blobSettings.SecretKey));
            }
            else
            {
                services.AddSingleton<IBlobStorage>(new GCStorage(Configuration.GetSection("ServiceAccount").Get<string>()));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (HostingEnvironment.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseGraphQL(new QueryMiddlewareOptions { EnableSubscriptions = false });
            app.UseGraphiQL();
            app.UsePlayground();
            app.UseVoyager();
        }
    }
}
