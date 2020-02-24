using System.Net;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto.User;
using Microsoft.AspNetCore.Authorization;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyApi.Models;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class G2Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IG2Service _g2Service;
        private readonly IAzureSearchService _azureSearchService;

        public G2Controller(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
            _g2Service = services.GetService<IG2Service>();
            _azureSearchService = services.GetService<IAzureSearchService>();
        }





        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> SearchSubscribers([FromBody] G2Dto g2)
        {
            var rVal = await _g2Service.CreateG2Async(g2);
            return Ok(rVal);
        }




        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("query")]
        public async Task<IActionResult> SearchSubscribers(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0, double lat = 0, double lng = 0)
        {
            var rVal = await _g2Service.SearchG2Async(limit, offset, sort, order, keyword,radius,lat,lng);
            return Ok(rVal);
        }


    }
}