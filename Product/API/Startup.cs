using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
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
using UpDiddy.Helpers;
using UpDiddyLib.Shared;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;


namespace UpDiddyApi
{
    public class Startup
    {
        public static string ScopeRead;
        public static string ScopeWrite;
        public IConfigurationRoot Configuration { get; set; }
 

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
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

            // Get the connection string from the Azure secret vault
            var SqlConnection = Configuration["CareerCircleSqlConnection"];                     
            services.AddDbContext<UpDiddyDbContext>(options => options.UseSqlServer(SqlConnection));
   
            // Add Dependency Injection for the configuration object
            services.AddSingleton<IConfiguration>(Configuration);
            // Add System Email   
            services.AddSingleton<ISysEmail>(new SysEmail(Configuration));

            // Add syslog
            services.AddScoped<ISysLog, SysLog>();

            // Add framework services.
            services.AddMvc();
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


            // Add Health check TODO remove this temp code 
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.SystemHealth(), Cron.MinuteInterval(5));

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
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            // Add App Insights Logging
            loggerFactory.AddApplicationInsights(app.ApplicationServices, Microsoft.Extensions.Logging.LogLevel.Warning);

            ScopeRead = Configuration["AzureAdB2C:ScopeRead"];
            ScopeWrite = Configuration["AzureAdB2C:ScopeWrite"];

            app.UseAuthentication();

            app.UseHangfireDashboard("/dashboard");
            app.UseHangfireServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
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
