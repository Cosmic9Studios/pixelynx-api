using AssetStore.Api.Settings;
using AssetStore.Api.Types;
using AssetStore.Data.BlobStorage;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Voyager;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddControllers();
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

            if (HostingEnvironment.EnvironmentName == "Development")
            {
                var blobSettings = new BlobSettings();
                Configuration.GetSection("BlobStorage").Bind(blobSettings);    
    
                services.AddSingleton<IBlobStorage>(new AmazonS3(blobSettings.Address, blobSettings.AccessKey, blobSettings.SecretKey));
            }
            else
            {
                services.Configure<AccountSettings>(options => Configuration.GetSection("Account").Bind(options));
                services.AddTransient<IBlobStorage, GCStorage>();
            }

            Configuration.GetSection("Minio").Bind(new MinioSettings());
            services.Configure<MinioSettings>(Configuration.GetSection("Minio"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (HostingEnvironment.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseGraphQL(new QueryMiddlewareOptions { EnableSubscriptions = false });
            app.UseGraphiQL();
            app.UsePlayground();
            app.UseVoyager();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
