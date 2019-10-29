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
using System.Net.Http;
using UpDiddyLib.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections;
using UpDiddyLib.Dto;
using UpDiddyLib.Shared;
using Microsoft.Net.Http.Headers;
using UpDiddy.Api;
using UpDiddy.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using React.AspNet; 
using DeviceDetectorNET;
using UpDiddy.Services;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ExceptionHandling;
 
 

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
            services.AddHttpContextAccessor();

			services.AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName)
				.AddChakraCore();

			services.AddReact(); 
            
            if (!Boolean.Parse(Configuration["Environment:IsPreliminary"]))
            {
                services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })                
                .AddAzureAdB2C(options => Configuration.Bind("Authentication:AzureAdB2C:Live", options))
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = "/Home/Forbidden";
                    options.Cookie.Path = "/";
                    options.SlidingExpiration = false;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                    options.Cookie.Expiration = TimeSpan.FromMinutes(int.Parse(Configuration["Cookies:MaxLoginDurationMinutes"]));
                });
            }
            else
            {
                services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddAzureAdB2C(options => Configuration.Bind("Authentication:AzureAdB2C:Pre", options))
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = "/Home/Forbidden";
                    options.Cookie.Path = "/";
                    options.SlidingExpiration = false;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                    options.Cookie.Expiration = TimeSpan.FromMinutes(int.Parse(Configuration["Cookies:MaxLoginDurationMinutes"]));
                });
            }

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies 
                // is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            #region AddLocalization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddCors(o => o.AddPolicy("UnifiedCors", builder =>
            {
                builder.WithOrigins("*")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddMemoryCache();
            services.AddMvc(config => {
                config.Filters.Add(typeof(CCExceptionFilter));
            })
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsRecruiterPolicy", policy => policy.AddRequirements(new GroupRequirement("Recruiter")));
                options.AddPolicy("IsCareerCircleAdmin", policy => policy.AddRequirements(new GroupRequirement("Career Circle Administrator")));
                options.AddPolicy("IsUserAdmin", policy => policy.AddRequirements(new GroupRequirement("Career Circle User Admin"))); 
            });

            services.AddSingleton<IAuthorizationHandler, ApiGroupAuthorizationHandler>();
            #endregion

            // Add Dependency Injection for the configuration object
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddHttpClient(Constants.HttpGetClientName);
            services.AddHttpClient(Constants.HttpPostClientName);
            services.AddHttpClient(Constants.HttpPutClientName);
            services.AddHttpClient(Constants.HttpDeleteClientName);              

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

       
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
            });


            // Add Redis session cahce 
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("redis:name");
                options.Configuration = Configuration.GetValue<string>("redis:host");
            });

            // Add Api
            services.AddScoped<IApi, ApiUpdiddy>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IButterCMSService, ButterCMSService>();
            services.AddScoped<ISysEmail, SysEmail>();



            services.AddDetection();

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
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
            else if(!Boolean.Parse(Configuration["Environment:IsPreliminary"]))
            {  
                app.UseExceptionHandler("/Home/Error");
                app.UseRewriter(new RewriteOptions().Add(new RedirectWwwRule()));
            } 
 
            // Initialise ReactJS.NET. Must be before static files.
            app.UseReact(config =>
            {
				config
					.SetReuseJavaScriptEngines(true)
					.SetLoadBabel(false)
					.SetLoadReact(false)
					.AddScriptWithoutTransform("~/js/runtime.js")
					.AddScriptWithoutTransform("~/js/vendor.js")
					.AddScriptWithoutTransform("~/js/components.js");
            });

            app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

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

            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path == "/signout-oidc" && !ctx.Request.Query["skip"].Any())
                {
                    var location = ctx.Request.Path + ctx.Request.QueryString + "&skip=1";
                    ctx.Response.StatusCode = 200;
                    var html = $@"
                        <html><head>
                            <meta http-equiv='refresh' content='0;url={location}' />
                        </head></html>";
                    await ctx.Response.WriteAsync(html);
                    return;
                }

                await next();

                if (ctx.Request.Path == "/signin-oidc" && ctx.Response.StatusCode == 302)
                {
                    var location = ctx.Response.Headers["location"];
                    ctx.Response.StatusCode = 200;
                    var html = $@"
                        <html><head>
                            <meta http-equiv='refresh' content='0;url={location}' />
                        </head></html>";
                    await ctx.Response.WriteAsync(html);
                }
            });


            app.UseSession();
            app.UseAuthentication();
            app.UseCors("UnifiedCors");

            // custom middleware for device detection
            app.Use((context, next) =>
            {
                var dd = new DeviceDetector(context.Request.Headers["User-Agent"].ToString());
                dd.Parse();
                string deviceType = dd.IsMobile() ? "is-mobile" : "is-desktop";
                context.Session.SetString("Device-Type", deviceType);
                return next();
            });

            app.UseCookiePolicy();

            // TODO - Change template action below to index upon site launch.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "sitemap",
                    "sitemap.xml",
                    new { controller = "Sitemap", action = "SiteMap" });
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    "NotFound",
                    "{*url}",
                    new { controller = "Home", action = "PageNotFound" });
            });         
        }
    }
}
