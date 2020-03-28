using Pixelynx.Api.Settings;
using Pixelynx.Api.Types;
using Pixelynx.Data.BlobStorage;
using C9S.Configuration.Variables;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pixelynx.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Pixelynx.Api.Security;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Pixelynx.Logic.Services;
using Microsoft.AspNetCore.Identity;
using System;
using Pixelynx.Data.Entities;
using Pixelynx.Logic.Settings;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Helpers;
using Pixelynx.Data.Models;
using Pixelynx.Data.Settings;
using Google.Cloud.Storage.V1;

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
            Configuration.ResolveVariables("${", "}");
            var connectionString = Configuration.GetConnectionString("Pixelynx");

            services.AddLogging(configure => configure.AddConsole());
            services.AddCors();
            services.AddControllers();

            // IOptions
            services.Configure<AuthSettings>(Configuration.GetSection("Auth"));
            services.Configure<StorageSettings>(Configuration.GetSection("Storage"));
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            
            // Environment specific services
            if (HostingEnvironment.EnvironmentName == "Development")
            {
                var blobSettings = new BlobSettings();
                Configuration.GetSection("BlobStorage").Bind(blobSettings);    
    
                services.AddSingleton<IBlobStorage>(new AmazonS3(blobSettings.Address, blobSettings.AccessKey, blobSettings.SecretKey));
            }
            else
            { 
                var urlSigner = GCPHelper.GetUrlSigner().Result;
                services.AddSingleton<UrlSigner>(urlSigner);
                services.AddSingleton<IBlobStorage>(new GCStorage(urlSigner));
            }

            // Services
            services.AddSingleton<DbContextFactory>(new DbContextFactory(connectionString));
            services.AddDbContext<PixelynxContext>(options => options.UseNpgsql(connectionString), ServiceLifetime.Transient);
            services.AddSingleton<UnitOfWork>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<UploadService, UploadService>();

            // Order matters. This needs to be before AddAuthentication
            services.AddIdentity<UserEntity, Role>()
                .AddEntityFrameworkStores<PixelynxContext>()
                .AddDefaultTokenProviders();

            var key = Encoding.ASCII.GetBytes(Configuration["Auth:JWTSecret"]);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.IncludeErrorDetails = true;
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("assets:read", policy => policy.Requirements.Add(new HasScopeRequirement("assets:read")));
                options.AddPolicy("assets:write", policy => policy.Requirements.Add(new HasScopeRequirement("assets:write")));
            });

            services.Configure<IdentityOptions>(options =>  
            {  
                // Password settings  
                options.Password.RequireDigit = true;  
                options.Password.RequiredLength = 8;  
                options.Password.RequireNonAlphanumeric = false;  
                options.Password.RequireUppercase = true;  
                options.Password.RequireLowercase = false;  
                options.Password.RequiredUniqueChars = 6;  
  
                // Lockout settings  
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);  
                options.Lockout.MaxFailedAccessAttempts = 10;  
                options.Lockout.AllowedForNewUsers = true;  
  
                // User settings  
                options.User.RequireUniqueEmail = true;  
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            services.AddGraphQL(sp =>
                SchemaBuilder.New()
                    .AddServices(sp)
                    .AddAuthorizeDirectiveType()
                    .AddQueryType<QueryType>()
                    .Create(), 

                new QueryExecutionOptions
                {
                    TracingPreference = TracingPreference.Always,
                    IncludeExceptionDetails = true
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, PixelynxContext context)
        {
            context.Database.Migrate();

            if (HostingEnvironment.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseGraphQL(new QueryMiddlewareOptions { EnableSubscriptions = false });
            app.UseGraphiQL();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
