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
using Pixelynx.Data.Interfaces;
using Stripe;
using System.Threading.Tasks;
using VaultSharp.V1.AuthMethods.GoogleCloud;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using System.Collections.Generic;
using C9S.Configuration.HashicorpVault.Helpers;
using System.Diagnostics;
using Community.Microsoft.Extensions.Caching.PostgreSql;
using Pixelynx.Api.Filters;
using Npgsql;
using Pixelynx.Logic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Pixelynx.Api
{
    public class Startup
    {
        public IWebHostEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        private string GetConnectionString(DbContextFactory dbContextFactory)
        {
            dbContextFactory.CreateSession();
            return dbContextFactory.SessionConnectionString;
        }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            HostingEnvironment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Rook.RookOptions options = new Rook.RookOptions()
            {
                token = "eb79354d2b8f6c741e70289f80549489a9be1c9cf982a420878ac1d748dd4b8c"
            };
            Rook.API.Start(options);

            Configuration.ResolveVariables("${", "}");
            var connectionString = Configuration.GetConnectionString("Pixelynx");

            services.AddLogging(configure => configure.AddConsole());
            var serviceProvider = services.BuildServiceProvider();

            services.AddCors();
            services.AddControllers(options => {
                if (Debugger.IsAttached) {
                    options.Filters.Add(typeof(ApiFilterAttribute));
                }
            }); 

            services.AddDataLoaderRegistry();

            // IOptions
            services.Configure<StorageSettings>(Configuration.GetSection("Storage"));
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));

            IAuthMethodInfo authMethod = null;

            // Environment specific services
            if (HostingEnvironment.EnvironmentName == "Development")
            {
                var blobSettings = new BlobSettings();
                Configuration.GetSection("BlobStorage").Bind(blobSettings);    
    
                services.AddSingleton<IBlobStorage>(new AmazonS3(blobSettings.Address, blobSettings.AccessKey, blobSettings.SecretKey));
                authMethod = new TokenAuthMethodInfo("token");
            }
            else
            { 
                var urlSigner = AsyncHelper.RunSync(GCPHelper.GetUrlSigner);
                services.AddSingleton<UrlSigner>(urlSigner);
                services.AddSingleton<IBlobStorage>(new GCStorage(urlSigner));
                authMethod = new GoogleCloudAuthMethodInfo("my-iam-role", AsyncHelper.RunSync(GCPHelper.GetJwt));
            }

            var address = Configuration.GetSection("Vault:Address").Get<string>();
            var vaultClientSettings = new VaultClientSettings(address, authMethod);
            var vaultClient = new VaultClient(vaultClientSettings);
            var vaultService = new VaultService(vaultClient);

            StripeConfiguration.ApiKey = AsyncHelper.RunSync(vaultService.GetAuthSecrets).StripeSecretKey;

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var dbContextFactory = new DbContextFactory(connectionString, vaultService, loggerFactory);
            var context = dbContextFactory.CreateAdmin();
            
            context.Database.Migrate();

            services.AddSession();
            services.AddDistributedPostgreSqlCache(setup =>
            {
                setup.SchemaName = "public";
                setup.TableName = "session";
                setup.ConnectionString = GetConnectionString(dbContextFactory);
            });

            // Services
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IVaultClient>(vaultClient);
            services.AddSingleton<IVaultService>(vaultService);
            services.AddSingleton<IDbContextFactory, DbContextFactory>(options => dbContextFactory);
            services.AddTransient<PixelynxContext>(options => dbContextFactory.CreateReadWrite());
            services.AddSingleton<UnitOfWork>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<UploadService, UploadService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddSingleton<IPaymentService, PaymentService>();

            // On Migration
            services.AddDbContext<PixelynxContext>(optitons => dbContextFactory.CreateAdmin());

            // Order matters. This needs to be before AddAuthentication
            services.AddIdentityCore<UserEntity>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<PixelynxContext>()
                .AddDefaultTokenProviders();

            var key = Encoding.ASCII.GetBytes(AsyncHelper.RunSync(vaultService.GetAuthSecrets).JWTSecret);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                options.Password.RequireLowercase = false;

                // Lockout settings  
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);  
                options.Lockout.MaxFailedAccessAttempts = 10;  
                options.Lockout.AllowedForNewUsers = true;  
  
                // User settings 
                options.User.RequireUniqueEmail = true;  
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            services.TryAddScoped<SignInManager<UserEntity>>();
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            services.AddGraphQL(sp =>
                SchemaBuilder.New()
                    .AddServices(sp)
                    .AddAuthorizeDirectiveType()
                    .AddQueryType<GQLQuery>()
                    .AddMutationType<GQLMutation>()
                    .Create()
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (HostingEnvironment.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();
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
            app.UsePlayground();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
