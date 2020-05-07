using System;
using System.Collections.Generic;
using System.Linq;
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
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyLib.Domain.Models.B2B;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class B2BController : BaseApiController
    {
        private readonly IHiringManagerService _hiringManagerService;
        private readonly IPipelineService _pipelineService;

        public B2BController(IServiceProvider services)
        {
            _hiringManagerService = services.GetService<IHiringManagerService>();
            _pipelineService = services.GetService<IPipelineService>();
        }

        [HttpPut]
        [Authorize]
        //[Authorize(Policy = "IsHiringManager")]
        [Route("hiring-manager")]
        public async Task<IActionResult> UpdateHiringManager([FromBody] HiringManagerDto request)
        {
            //HM update
            var rVal = await _hiringManagerService.AddHiringManager(GetSubscriberGuid(), true);
            return Ok(rVal);
        }

        #region Pipeline Operations

        [HttpPost]
        [Authorize(Policy = "IsHiringManager")]
        [Route("pipelines")]
        public async Task<IActionResult> CreatePipeline([FromBody] PipelineDto pipelineDto)
        {
            Guid pipelineGuid = await _pipelineService.CreatePipelineForHiringManager(GetSubscriberGuid(), pipelineDto);
            return StatusCode(201, pipelineGuid);
        }

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("pipelines/{pipelineGuid:guid}")]
        public async Task<IActionResult> GetPipeline(Guid pipelineGuid)
        {
            var pipeline = await _pipelineService.GetPipelineForHiringManager(pipelineGuid, GetSubscriberGuid());
            return Ok(pipeline);
        }

        [HttpPut]
        [Authorize(Policy = "IsHiringManager")]
        [Route("pipelines/{pipelineGuid:guid}")]
        public async Task<IActionResult> UpdatePipeline(Guid pipelineGuid, [FromBody] PipelineDto pipelineDto)
        {
            pipelineDto.PipelineGuid = pipelineGuid;
            await _pipelineService.UpdatePipelineForHiringManager(GetSubscriberGuid(), pipelineDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Authorize(Policy = "IsHiringManager")]
        [Route("pipelines/{pipelineGuid:guid}")]
        public async Task<IActionResult> DeletePipeline(Guid pipelineGuid)
        {
            await _pipelineService.DeletePipelineForHiringManager(GetSubscriberGuid(), pipelineGuid);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("pipelines")]
        public async Task<IActionResult> GetPipelines(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var pipelines = await _pipelineService.GetPipelinesForHiringManager(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(pipelines);
        }

        [HttpPost]
        [Authorize(Policy = "IsHiringManager")]
        [Route("pipelines/{pipelineGuid:guid}/profiles")]
        public async Task<IActionResult> AddProfilesToPipeline(Guid pipelineGuid, [FromBody] List<Guid> profileGuids)
        {
            List<Guid> pipelineProfileGuids = await _pipelineService.AddPipelineProfilesForHiringManager(GetSubscriberGuid(), pipelineGuid, profileGuids);
            return StatusCode(201, pipelineProfileGuids);
        }

        [HttpDelete]
        [Authorize(Policy = "IsHiringManager")]
        [Route("pipelines/profiles")]
        public async Task<IActionResult> DeleteProfilesFromPipeline([FromBody] List<Guid> pipelineProfileGuids)
        {
            await _pipelineService.DeletePipelineProfilesForHiringManager(GetSubscriberGuid(), pipelineProfileGuids);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("pipelines/{pipelineGuid:guid}/profiles")]
        public async Task<IActionResult> GetPipelineProfiles(Guid pipelineGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var pipelineProfiles = await _pipelineService.GetPipelineProfilesForHiringManager(pipelineGuid, GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(pipelineProfiles);
        }

        #endregion
    }
}