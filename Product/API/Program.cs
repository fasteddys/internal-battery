using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureAppConfiguration((context, config) =>
                {                    
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    var builtConfig = config.Build();

                    config.AddAzureKeyVault(
                    builtConfig["Vault"],
                    builtConfig["ClientId"],
                    builtConfig["ClientSecret"]);

                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }


 

    }
}
