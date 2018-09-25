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

namespace UpDiddyApi
{
 

    public class Startup
    {
        public static string ScopeRead;
        public static string ScopeWrite;
        private IConfiguration _vaultConfig;
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            _vaultConfig = configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                // Add in the azure vault entries 
                .AddConfiguration(configuration);
                
            builder.AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            
            Configuration = builder.Build();
           
        }

        public IConfigurationRoot Configuration { get; set; }

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
            var SqlConnection = _vaultConfig["CareerCircleSqlConnection"];         
            services.AddDbContext<UpDiddyDbContext>(options => options.UseSqlServer(SqlConnection));

            services.AddHangfire(x => x.UseSqlServerStorage(SqlConnection));



            // Add Dependency Injection for the configuration object
            services.AddSingleton<IConfiguration>(Configuration);

            // Add framework services.
            services.AddMvc();
            // Add AutoMapper 
            AutoMapperConfiguration.Init();
            services.AddAutoMapper(typeof(Startup));            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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
    }
}
