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
using UpDiddyLib.Shared;


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
            .AddCookie();

            #region AddLocalizationß
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
            #endregion

            // Add Dependency Injection for the configuration object
            services.AddSingleton<IConfiguration>(Configuration);



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

            Console.WriteLine("Redis name: " + Configuration.GetValue<string>("redis:name") + ", Redis host: " + Configuration.GetValue<string>("redis:host"));
            Console.WriteLine("B2C Secret: " + Configuration.GetValue<string>("Authentication:AzureAdB2C:ClientSecret") + ", B2C ID: " + Configuration.GetValue<string>("Authentication:AzureAdB2C:ClientId"));

            // Add Redis session cahce
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("redis:name");
                options.Configuration = Configuration.GetValue<string>("redis:host");
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            /* Implements HTTPS redirect
            var options = new RewriteOptions().AddRedirectToHttps();
            app.UseRewriter(options);
            */

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseRewriter(new RewriteOptions().Add(new RedirectWwwRule()));
            }

            if(env.IsProduction())
                app.UseRewriter(new RewriteOptions().Add(new RedirectWwwRule()));

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

            app.Use((context, next) =>
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = "https://login.microsoftonline.com";
                return next.Invoke();
            });
        
            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthentication();

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
