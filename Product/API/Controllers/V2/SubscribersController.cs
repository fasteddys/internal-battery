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
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class SubscribersController : BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly ISubscriberService _subscriberService;
        private readonly IAzureSearchService _azureSearchService;
        private readonly IHiringManagerService _hiringManagerService;

        public SubscribersController(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
            _subscriberService = services.GetService<ISubscriberService>();
            _azureSearchService = services.GetService<IAzureSearchService>();
            _hiringManagerService = services.GetService<IHiringManagerService>();
        }

        [HttpPost]
        //[MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("new-subscriber-registration")]
        public async Task<IActionResult> NewSubscriberRegistration([FromBody] SubscriberDto subscriberDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var existingSubscriber = await _subscriberService.GetSubscriberByEmail(subscriberDto.Email);
            if (existingSubscriber != null)
                return Conflict();

            var newSubscriberGuid = await _subscriberService.CreateSubscriberAsync(subscriberDto);
            return StatusCode(201, newSubscriberGuid);
        }

        [HttpPut]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("sync-auth0-userid")]
        public async Task<IActionResult> SyncAuth0UserId([FromBody] SubscriberDto subscriberDto)
        {
            await _subscriberService.SyncAuth0UserId(subscriberDto.SubscriberGuid, subscriberDto.Auth0UserId);
            return StatusCode(200);
        }
                
        [HttpPut]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("{subscriber:guid}/track-sign-in")]
        public async Task<IActionResult> TrackSignIn(Guid subscriber)
        {
            await _subscriberService.TrackSubscriberSignIn(subscriber);
            return StatusCode(200);
        }

        [HttpPost]
        [Authorize]
        [Route("existing-subscriber-campaign-signup")]
        public async Task<IActionResult> ExistingUserCampaignSignup([FromBody] CreateUserDto createUserDto)
        {
            createUserDto.SubscriberGuid = GetSubscriberGuid();
            await _subscriberService.ExistingSubscriberSignUp(createUserDto);
            return StatusCode(204);
        }


        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        [Route("query")]
        public async Task<IActionResult> SearchSubscribers(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*")
        {         
            var rVal = await _subscriberService.SearchSubscribersAsync(limit, offset, sort, order, keyword);
            return Ok(rVal);
        }

        [HttpPut]
        [Authorize]
        [Route("hiring-manager")]
        public async Task<IActionResult> AddHiringManager()
        {
            var rVal = await _hiringManagerService.AddHiringManager(GetSubscriberGuid(), true);
            return Ok(rVal);
        }





    }
}