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
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Exceptions;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class B2BController : BaseApiController
    {
        private readonly IHiringManagerService _hiringManagerService;
        private readonly IInterviewRequestService _interviewRequestService;
        private readonly IProfileService _profileService;

        public B2BController(IHiringManagerService hiringManagerService, IInterviewRequestService interviewRequestService, IProfileService profileService)
        {
            _hiringManagerService = hiringManagerService;
            _interviewRequestService = interviewRequestService;
            _profileService = profileService;
        }

        [HttpPut]
        [Authorize]
        //[Authorize(Policy = "IsHiringManager")] ???
        [Route("hiring-manager")]
        public async Task<IActionResult> UpdateHiringManager([FromBody] HiringManagerDto request)
        {
            await _hiringManagerService.UpdateHiringManager(GetSubscriberGuid(), request);
            return Ok();
        }

        [HttpPost("requestInterview/{profileGuid}")]
        public async Task<IActionResult> SubmitInterviewRequest(Guid profileGuid)
        {
            var hiringManager = Guid.Empty;
            // TODO: await _hiringManagerService.GetBySubscriberGuid(GetSubscriberGuid());
            // TODO:     Above method should return HiringManagerDto
            // TODO:     HiringManagerDto should include Guid identifier.

            _interviewRequestService.SubmitInterviewRequest(hiringManager, profileGuid);
        }
    }
}