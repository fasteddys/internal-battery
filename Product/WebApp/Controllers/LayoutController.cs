using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ButterCMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Helpers.Braintree;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class LayoutController : BaseController
    {
        private readonly IConfiguration _configuration;

        public LayoutController(IApi api, IConfiguration configuration) : base(api)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> GetDataAsync()
        {
            var butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
            var keys = new string[1] { "content" };
            var dictionary = new Dictionary<string, string>
            {
                { "keys", "menu_item" }
            };
            var contentFields = await butterClient.RetrieveContentFieldsJSONAsync(keys, dictionary);
            return Content(contentFields);
        }
    }
}
