using System.Net;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
using Microsoft.AspNetCore.Authorization;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyApi.Models;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class CommunityGroupsController : BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly ICommunityGroupService _communityGroupService;
        public CommunityGroupsController(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
            _communityGroupService = services.GetService<ICommunityGroupService>();
        }

        [HttpGet]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("get-community-groups")]
        public async Task<IActionResult> GetAllCommunityGroups()
        {
            return Ok(await _communityGroupService.GetAllCommunityGroups());
        }

        [HttpPost]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("add-community-group")]
        public async Task<IActionResult> AddCommunityGroup([FromBody] UpDiddyLib.Domain.Models.CommunityGroupDto communityGroupDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var existingCommunityGroup = await _communityGroupService.GetCommunityGroupByName(communityGroupDto.Name);
            if (existingCommunityGroup != null)
                return Conflict();

            var newCommunityGroupGuid = await _communityGroupService.CreateCommunityGroup(communityGroupDto);
            return StatusCode(201, newCommunityGroupGuid);
        }

        [HttpPost]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("update-community-group")]
        public async Task<IActionResult> UpdateCommunityGroup([FromBody] UpDiddyLib.Domain.Models.CommunityGroupDto communityGroupDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await _communityGroupService.UpdateCommunityGroup(communityGroupDto);
            return Ok();
        }

        [HttpPost]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("delete-community-group")]
        public async Task<IActionResult> DeleteCommunityGroup([FromBody] Guid communityGroupGuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await _communityGroupService.DeleteCommunityGroup(communityGroupGuid);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("query-community-group")]
        public async Task<IActionResult> SearchCommunityGroups(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*")
        {
            var rVal = await _communityGroupService.SearchCommunityGroupsAsync(limit, offset, sort, order, keyword);
            return Ok(rVal);
        }




        [HttpGet]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("get-subscribers-in-community-group")]
        public async Task<IActionResult> GetSubscribersInCommunityGroup(Guid communityGroupSubscriberGuid)
        {        
            return Ok(await _communityGroupService.GetCommunityGroupSubscribers(communityGroupSubscriberGuid));
        }

        [HttpPost]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("add-subscriber-to-community-group")]
        public async Task<IActionResult> AddSubscriberToCommunityGroup([FromBody] UpDiddyLib.Domain.Models.CommunityGroupSubscriberDto communityGroupSubscriberDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var existingCommunityGroup = await _communityGroupService.GetCommunityGroupSubscriber(communityGroupSubscriberDto.CommunityGroupSubscriberGuid);
            if (existingCommunityGroup != null)
                return Conflict();

            var newCommunityGroupGuid = await _communityGroupService.CreateCommunityGroupSubscriber(communityGroupSubscriberDto);
            return StatusCode(201, newCommunityGroupGuid);
        }

        [HttpPost]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("remove-subscriber-from-community-group")]
        public async Task<IActionResult> RemoveSubscriberFromCommunityGroup([FromBody] Guid communityGroupSubscriberGuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await _communityGroupService.DeleteCommunityGroupSubscriber(communityGroupSubscriberGuid);
            return Ok();
        }

    }
}