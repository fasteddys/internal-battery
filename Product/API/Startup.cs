using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using AutoMapper;
using UpDiddyLib.Dto;
using UpDiddyApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using UpDiddyApi.Workflow;
using Hangfire.SqlServer;
using System;
using UpDiddyLib.Helpers;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using UpDiddyLib.Helpers;
using UpDiddyLib.Shared;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Sinks.ApplicationInsights;
using UpDiddyLib.Serilog.Sinks;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using UpDiddyApi.Business.Resume;
using System.Collections.Generic;
using UpDiddyApi.Business.Graph;
using System.Security.Claims;
using UpDiddyApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UpDiddyApi.Helpers.SignalR;

namespace UpDiddyApi
{
    public class Startup
    {
        public static string ScopeRead;
        public static string ScopeWrite;
        public IConfigurationRoot Configuration { get; set; }

        public Serilog.ILogger Logger { get; } 

        public ISysEmail SysEmail { get; }

        public Startup(IHostingEnvironment env, IConfiguration configuration )
        {
            // Note: please refer to UpDiddyDbContext if this logic needs to be updated (configuration)
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // if environment is set to development then add user secrets
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            // if environment is set to staging or production then add vault keys
            var config = builder.Build();
            if(env.IsStaging() || env.IsProduction())
            {
                builder.AddAzureKeyVault(config["Vault:Url"], 
                    config["Vault:ClientId"],
                    config["Vault:ClientSecret"], 
                    new KeyVaultSecretManager());
            }

            Configuration = builder.Build();

            SysEmail = new SysEmail(Configuration);

            // directly add Application Insights and SendGrid to access Key Vault Secrets
            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.ApplicationInsightsTraces(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"])
                .WriteTo
                    .SendGrid(LogEventLevel.Fatal, Configuration["SysEmail:ApiKey"], Configuration["SysEmail:SystemErrorEmailAddress"])
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
          
            services.AddSingleton<Serilog.ILogger>(Logger);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority = $"https://login.microsoftonline.com/tfp/{Configuration["AzureAdB2C:Tenant"]}/{Configuration["AzureAdB2C:Policy"]}/v2.0/";
                jwtOptions.Audience = Configuration["AzureAdB2C:ClientId"];
                jwtOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = AuthenticationFailed
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsRecruiterPolicy", policy => policy.AddRequirements(new GroupRequirement("Recruiter")));
            });
            services.AddSingleton<IAuthorizationHandler, GroupAuthorizationHandler>();

            // Get the connection string from the Azure secret vault
            var SqlConnection = Configuration["CareerCircleSqlConnection"];                     
            services.AddDbContext<UpDiddyDbContext>(options => options.UseSqlServer(SqlConnection));
   
            // Add Dependency Injection for the configuration object
            services.AddSingleton<IConfiguration>(Configuration);
            // Add System Email   
            services.AddSingleton<ISysEmail>(new SysEmail(Configuration));

            List<string> origins = Configuration.GetSection("Cors:Origins").Get<List<string>>();
            // Shows UseCors with CorsPolicyBuilder.
            services.AddCors(o => o.AddPolicy("Cors", builder =>
            {
                builder.WithOrigins(origins.ToArray())
                       .AllowCredentials()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));


            // Add framework services.
            // the 'ignore' option for reference loop handling was implemented to prevent circular errors during serialization 
            // (e.g. SubscriberDto contains a collection of EnrollmentDto objects, and the EnrollmentDto object has a reference to a SubscriberDto)
            services.AddMvc().AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // Add SignalR
            services.AddSignalR();

            // Add AutoMapper 
            services.AddAutoMapper(typeof(UpDiddyApi.Helpers.AutoMapperConfiguration));

            // Configure Hangfire 
            var HangFireSqlConnection = Configuration["CareerCircleSqlConnection"]; 
            services.AddHangfire(x => x.UseSqlServerStorage(HangFireSqlConnection));
            // Have the workflow monitor run every minute 
            JobStorage.Current = new SqlServerStorage(HangFireSqlConnection);
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.ReconcileFutureEnrollments(), Cron.Daily);

            // PromoCodeRedemption cleanup
            int promoCodeRedemptionCleanupScheduleInMinutes = 5;
            int promoCodeRedemptionLookbackInMinutes = 30;
            int.TryParse(Configuration["PromoCodeRedemptionCleanupScheduleInMinutes"].ToString(), out promoCodeRedemptionCleanupScheduleInMinutes);
            int.TryParse(Configuration["PromoCodeRedemptionLookbackInMinutes"].ToString(), out promoCodeRedemptionLookbackInMinutes);
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.DoPromoCodeRedemptionCleanup(promoCodeRedemptionLookbackInMinutes), Cron.MinuteInterval(promoCodeRedemptionCleanupScheduleInMinutes));            
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

            services.AddTransient<ISovrenAPI, Sovren>();
            services.AddHttpClient<ISovrenAPI,Sovren>();

            services.AddTransient<IB2CGraph, B2CGraphClient>();
            services.AddHttpClient<IB2CGraph, B2CGraphClient>();

            // Configure SnapshotCollector from application settings
            // TODO Uncomment test 
            //services.Configure<SnapshotCollectorConfiguration>(Configuration.GetSection(nameof(SnapshotCollectorConfiguration)));
            // Add SnapshotCollector telemetry processor.
            // TODO Uncomment test 
            //services.AddSingleton<ITelemetryProcessorFactory>(sp => new SnapshotCollectorTelemetryProcessorFactory(sp));

            // Add Redis session cahce
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("redis:name");
                options.Configuration = Configuration.GetValue<string>("redis:host");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog(Logger);

            ScopeRead = Configuration["AzureAdB2C:ScopeRead"];
            ScopeWrite = Configuration["AzureAdB2C:ScopeWrite"];

            app.UseAuthentication();

            app.UseCors("Cors");

            app.UseHangfireDashboard("/dashboard");
            app.UseHangfireServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Added for SignalR
            app.UseSignalR(routes =>
            {
                routes.MapHub<ClientHub>("/clienthub");
            });

        }

        private Task AuthenticationFailed(AuthenticationFailedContext arg)
        {
            // For debugging purposes only!
            var s = $"AuthenticationFailed: {arg.Exception.Message}";
            arg.Response.ContentLength = s.Length;
            arg.Response.Body.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
            return Task.FromResult(0);
        }

        private class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
                _serviceProvider = serviceProvider;

            public ITelemetryProcessor Create(ITelemetryProcessor next)
            {
                var snapshotConfigurationOptions = _serviceProvider.GetService<IOptions<SnapshotCollectorConfiguration>>();
                return new SnapshotCollectorTelemetryProcessor(next, configuration: snapshotConfigurationOptions.Value);
            }
        }
    }
}
