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
using UpDiddyApi.Workflow;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class G2Controller :  BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly IG2Service _g2Service;
        private readonly IAzureSearchService _azureSearchService;
        private readonly IHangfireService _hangfireService;

        public G2Controller(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
            _g2Service = services.GetService<IG2Service>();
            _azureSearchService = services.GetService<IAzureSearchService>();
            _hangfireService = services.GetService<IHangfireService>();
        }




        [HttpDelete]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("{subscriberGuid}")]
        public async Task<IActionResult> DeleteSubscriberFromIndex(Guid subscriberGuid)
        {
 
            _g2Service.DeleteSubscriber(subscriberGuid);
            return StatusCode(204);
        }

 
        /// <summary>
        /// Add new subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("{subscriberGuid}")]
        public async Task<IActionResult> AddNewSubscriber(Guid subscriberGuid)
        {
            _g2Service.AddSubscriber(subscriberGuid);
            return StatusCode(200);
        }



        /// <summary>
        /// Re-index subsriber.  This operation will update as well as create documents in the 
        /// azure g2 index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("{subscriberGuid}")]
        public async Task<IActionResult> ReindexSubscriber(Guid subscriberGuid)
        {
            _g2Service.IndexSubscriber(subscriberGuid);
            return StatusCode(200);
        }




        [HttpPost]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("index")]
        public async Task<IActionResult> IndexSubscribers([FromBody] G2SDOC g2)
        {
            var rVal = await _g2Service.IndexG2Async(g2);
            return Ok(rVal);
        }



        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("query")]
        public async Task<IActionResult> SearchG2(int cityId,  int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0 )
        {          
            var rVal = await _g2Service.SearchG2Async( GetSubscriberGuid() , cityId, limit, offset, sort, order, keyword,radius);
            return Ok(rVal);
        }


    }
}