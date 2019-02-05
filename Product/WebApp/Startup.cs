using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Rewrite;
using UpDiddy.Helpers.RewriteRules;
using Polly;
using System.Net.Http;
using Polly.Extensions.Http;
using UpDiddyLib.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Polly.Registry;
using Polly.Caching;
using System.Collections;
using UpDiddyLib.Dto;
using UpDiddyLib.Shared;
using Microsoft.Net.Http.Headers;
using UpDiddy.Api;
using UpDiddy.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UpDiddy
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // if environment is set to development then add user secrets
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            // if environment is set to staging or production then add vault keys
            var config = builder.Build();
            if (env.IsStaging() || env.IsProduction())
            {
                builder.AddAzureKeyVault(config["Vault:Url"],
                    config["Vault:ClientId"],
                    config["Vault:ClientSecret"],
                    new KeyVaultSecretManager());
            }

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAdB2C(options => Configuration.Bind("Authentication:AzureAdB2C", options))
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/Home/Forbidden";
                options.Cookie.Path = "/";
                options.SlidingExpiration = false;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                options.Cookie.Expiration = TimeSpan.FromMinutes(int.Parse(Configuration["Cookies:MaxLoginDurationMinutes"]));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsRecruiterPolicy", policy => policy.AddRequirements(new GroupRequirement("Recruiter")));
                options.AddPolicy("IsCareerCircleAdmin", policy => policy.AddRequirements(new GroupRequirement("Career Circle Administrator")));
                options.AddPolicy("IsUserAdmin", policy => policy.AddRequirements(new GroupRequirement("Career Circle User Admin")));
            });
            services.AddSingleton<IAuthorizationHandler, ApiGroupAuthorizationHandler>();
       

            #region AddLocalization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddCors(o => o.AddPolicy("UnifiedCors", builder =>
            {
                builder.WithOrigins("*")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
            #endregion

            // Add Dependency Injection for the configuration object
            services.AddSingleton<IConfiguration>(Configuration);

            // Add Polly 

            // Create Policies  
            int PollyRetries = int.Parse(Configuration["Polly:Retries"]);
            int PollyBreakDurationInMinutes = int.Parse(Configuration["Polly:BreakDurationInMinutes"]);
            // Define default api policy with async retries and exponential backoff            
            var ApiGetPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(PollyRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            // Define a policy without retries for non idempotenic operations
            // FMI: https://www.stevejgordon.co.uk/httpclientfactory-using-polly-for-transient-fault-handling
            var ApiPostPolicy = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();
            var ApiPutPolicy = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();
            var ApiDeletePolicy = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();

            services.AddHttpClient(Constants.HttpGetClientName)
              .AddPolicyHandler(ApiGetPolicy);

            services.AddHttpClient(Constants.HttpPostClientName)
                          .AddPolicyHandler(ApiPostPolicy);

            services.AddHttpClient(Constants.HttpPutClientName)
                          .AddPolicyHandler(ApiPutPolicy);

            services.AddHttpClient(Constants.HttpDeleteClientName)
              .AddPolicyHandler(ApiDeletePolicy);

            // Configure supported cultures and localization options
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                     new CultureInfo("en-US"),
                     new CultureInfo("fr")
                };

                // State what the default culture for your application is. This is used if no specific culture
                // can be determined for a given request.
                options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings (that we have localized resources for).
                options.SupportedUICultures = supportedCultures;
            });

            // Add Redis session cahce
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("redis:name");
                options.Configuration = Configuration.GetValue<string>("redis:host");
            });

            services.AddHttpContextAccessor();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
            });
            // Enable session DI
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Add Api
            services.AddScoped<IApi, ApiUpdiddy>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseRewriter(new RewriteOptions().Add(new RedirectWwwRule()));
            }

            var supportedCultures = new[]
                {
                new CultureInfo("en-US"),
                new CultureInfo("en-AU"),
                new CultureInfo("en-GB"),
                new CultureInfo("en"),
                new CultureInfo("es-ES"),
                new CultureInfo("es-MX"),
                new CultureInfo("es"),
                new CultureInfo("fr-FR"),
                new CultureInfo("fr")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("fr"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            // set the cache-control header to 24 hours
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }
            });

            app.UseSession();
            app.UseAuthentication();
            app.UseCors("UnifiedCors");

            // TODO - Change template action below to index upon site launch.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    "NotFound",
                    "{*url}",
                    new { controller = "Home", action = "PageNotFound" }
                );
            });
        }
    }
}
