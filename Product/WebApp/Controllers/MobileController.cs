using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UpDiddy.Controllers
{
    public class MobileController : BaseController
    {

        AzureAdB2COptions AzureAdB2COptions;
        private readonly IStringLocalizer<MobileController> _localizer;
        private readonly IConfiguration _configuration;

        public MobileController(IOptions<AzureAdB2COptions> azureAdB2COptions, IStringLocalizer<MobileController> localizer, IConfiguration configuration,  IHttpClientFactory httpClientFactory, IDistributedCache cache) : base(azureAdB2COptions.Value, configuration, httpClientFactory, cache)
        {
            _localizer = localizer;
            AzureAdB2COptions = azureAdB2COptions.Value;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return RedirectPermanent("https://www.careercircle.com/Home/Index");
        }
    }
}
