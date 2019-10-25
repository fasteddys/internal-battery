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
using UpDiddyLib.Helpers;
using Microsoft.Extensions.Caching.Distributed;
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
using System.Threading.Tasks;

namespace UpDiddy
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IHostingEnvironment env)
        {
            // store this so that we can access it in ConfigureServices
            HostingEnvironment = env;

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
         
            string domain = $"https://{Configuration["Auth0:Domain"]}";

            services.Configure<CookiePolicyOptions>(options =>
           {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
               options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
           });

            // Add Auth0 authentication services
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options => {
                options.LoginPath = "/Session/SignIn";
                options.LogoutPath = "/Session/SignOut";
                options.AccessDeniedPath = "/Session/AccessDenied";
                options.ExpireTimeSpan = new TimeSpan(1, 0, 0); // ensure that this matches the Auth0 application's "JWT Expiration" value
            })
            .AddOpenIdConnect("Auth0", options =>
            {
                // Set the authority to your Auth0 domain
                options.Authority = $"https://{Configuration["Auth0:Domain"]}";

                // Configure the Auth0 Client ID and Client Secret
                options.ClientId = Configuration["Auth0:ClientId"];
                options.ClientSecret = Configuration["Auth0:ClientSecret"];

                // Set response type to code
                options.ResponseType = "code";

                // Configure the scope
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");


                // Set the callback path, so Auth0 will call back to http://localhost:5000/callback
                // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
                options.CallbackPath = new PathString("/callback");
                options.GetClaimsFromUserInfoEndpoint = true;

                // Configure the Claims Issuer to be Auth0
                options.ClaimsIssuer = "Auth0";

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Properties.Items.ContainsKey("connection"))
                            context.ProtocolMessage.SetParameter("connection", context.Properties.Items["connection"]);

                        return Task.FromResult(0);
                    },
                    // handle the logout redirection
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = $"https://{Configuration["Auth0:Domain"]}/v2/logout?client_id={Configuration["Auth0:ClientId"]}";

                        var postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                // transform to absolute
                                var request = context.Request;
                                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                            }
                            logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                        }

                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }
                };
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
            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(CCExceptionFilter));
            })
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsRecruiterPolicy", policy => policy.Requirements.Add(new HasScopeRequirement(new string[] { "Recruiter" }, domain)));
                options.AddPolicy("IsCareerCircleAdmin", policy => policy.Requirements.Add(new HasScopeRequirement(new string[] { "Career Circle Administrator" }, domain)));
                options.AddPolicy("IsRecruiterOrAdmin", policy => policy.Requirements.Add(new HasScopeRequirement(new string[] { "Recruiter", "Career Circle Administrator" }, domain)));
            });

            //services.AddSingleton<IAuthorizationHandler, ApiGroupAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

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

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddJsonOptions(options =>
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
            else if (!Boolean.Parse(Configuration["Environment:IsPreliminary"]))
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

            app.UseSession();
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

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

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
