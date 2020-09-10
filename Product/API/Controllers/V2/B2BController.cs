using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.B2B;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class B2BController : BaseApiController
    {
        private readonly IHiringManagerService _hiringManagerService;
        private readonly IPipelineService _pipelineService;
        private readonly IInterviewRequestService _interviewRequestService;
        private readonly IG2Service _g2Service;
        private readonly ICareerTalentPipelineService _careerTalentPipelineService;

        public B2BController(IServiceProvider services)
        {
            _hiringManagerService = services.GetService<IHiringManagerService>();
            _pipelineService = services.GetService<IPipelineService>();
            _interviewRequestService = services.GetService<IInterviewRequestService>();
            _g2Service = services.GetService<IG2Service>();
            _careerTalentPipelineService = services.GetService<ICareerTalentPipelineService>();
        }


        #region Hiring Manager Crud

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("hiring-managers")]
        public async Task<IActionResult> GetHiringManager()
        {

            var rval = await _hiringManagerService.GetHiringManagerBySubscriberGuid(GetSubscriberGuid());
            return Ok(rval);
        }

        [HttpPut]
        [Authorize(Policy = "IsHiringManager")]
        [Route("hiring-managers")]
        public async Task<IActionResult> UpdateHiringManager([FromBody] HiringManagerDto request)
        {
            await _hiringManagerService.UpdateHiringManager(GetSubscriberGuid(), request);
            return Ok();
        }

        #endregion

        #region Hiring  Query Functions 

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("profiles/query")]
        public async Task<IActionResult> SearchG2(Guid cityGuid, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", Guid? sourcePartnerGuid = null, int radius = 0, bool? isWillingToRelocate = null, bool? isWillingToTravel = null, bool? isActiveJobSeeker = null, bool? isCurrentlyEmployed = null, bool? isWillingToWorkProBono = null)
        {
            var rVal = await _g2Service.HiringManagerSearchAsync(GetSubscriberGuid(), cityGuid, limit, offset, sort, order, keyword, sourcePartnerGuid, radius, isWillingToRelocate, isWillingToTravel, isActiveJobSeeker, isCurrentlyEmployed, isWillingToWorkProBono);
            return Ok(rVal);
        }

        #endregion Hiring Query Functions 

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

        #region Create Talent Pipeline

        [HttpGet("create-talent-pipeline")]
        public ActionResult<List<string>> GetCreateTalentPipelineQuestions()
            => _careerTalentPipelineService.GetQuestions();

        [HttpPost("create-talent-pipeline")]
        [Authorize(Policy = "IsHiringManager")]
        public async Task<IActionResult> PostCreateTalentPipelineQuestions([FromBody]CareerTalentPipelineDto careerTalentPipelineDto)
        {
            var hiringManager = await _hiringManagerService
               .GetHiringManagerBySubscriberGuid(GetSubscriberGuid());

            await _careerTalentPipelineService
                .SubmitCareerTalentPipeline(careerTalentPipelineDto, hiringManager);

            return Ok();
        }


        #endregion Create Talent Pipeline

        #region Interview Requests 

        [HttpPost("hiring-managers/request-interview/{profileGuid}")]
        [Authorize(Policy = "IsHiringManager")]
        public async Task<IActionResult> SubmitInterviewRequest(Guid profileGuid)
        {
            var hiringManager = await _hiringManagerService
                .GetHiringManagerBySubscriberGuid(GetSubscriberGuid());

            var interviewRequestId = await _interviewRequestService
                .SubmitInterviewRequest(hiringManager, profileGuid);

            return Ok(interviewRequestId);
        }


        #endregion

        #region Profiles 

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("profiles/{profileGuid}/candidate")]
        public async Task<IActionResult> GetCandidate360ProfileDetail(Guid profileGuid)
        {
            return Ok(await _hiringManagerService.GetCandidate360Detail(profileGuid));
        }

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("profiles/{candidateProfileGuid}")]
        public async Task<IActionResult> GetCandidateProfileDetail(Guid candidateProfileGuid)
        {
            var rval = await _hiringManagerService.GetCandidateProfileDetail(candidateProfileGuid);
            return Ok(rval);
        }

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("profiles/{candidateProfileGuid}/education-histories")]
        public async Task<IActionResult> GetCandidateEducationHistory(Guid candidateProfileGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var rval = await _hiringManagerService.GetCandidateEducationHistory(candidateProfileGuid, limit, offset, sort, order);
            return Ok(rval);
        }

        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("profiles/{candidateProfileGuid}/work-histories")]
        public async Task<IActionResult> GetCandidateWorkHistory(Guid candidateProfileGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var rval = await _hiringManagerService.GetCandidateWorkHistory(candidateProfileGuid, limit, offset, sort, order);
            return Ok(rval);
        }


        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("skills/profiles/{candidateProfileGuid}")]
        public async Task<IActionResult> GetCandidateSkills(Guid candidateProfileGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var rval = await _hiringManagerService.GetCandidateSkills(candidateProfileGuid, limit, offset, sort, order);
            return Ok(rval);
        }

        #endregion

        #region Candidate Indexing Admin Operations 

        // Admin functions will not be made public throught the APi gateway.  They are here for dev administration of the 
        // g2 profiles and azure index 

        /// <summary>
        /// Creates G2 profiles for all active subscriber/company combinations
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/index")]
        public async Task<IActionResult> addNewSubscribers()
        {
            // 
            _g2Service.G2AddNewSubscribers();
            return StatusCode(202);
        }


        /*   Todo when the time arises ....

        /// <summary>
        /// Deletes all g2 records from the azure index 
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/index")]
        public async Task<IActionResult> deleteG2()
        {

            _g2Service.G2IndexPurgeAsync();
            return StatusCode(202);
        }



        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/companies/{companyGuid}")]
        public async Task<IActionResult> DeleteCompanyFromIndex(Guid companyGuid)
        {

            _g2Service.G2DeleteCompanyAsync(companyGuid);
            return StatusCode(202);
        }


        /// <summary>
        /// Add new company.  This will create a new G2 Profile for every active subscriber for the specified company  
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/companies/{companyGuid}")]
        public async Task<IActionResult> AddNewCompany(Guid companyGuid)
        {
            _g2Service.G2AddCompanyAsync(companyGuid);
            return StatusCode(202);
        }

        /// <summary>
        /// remove the subcribers profiles and remove them from the azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("admin/profiles/subscriber/{subscriberGuid}")]
        public async Task<IActionResult> DeleteSubscriberFromIndex(Guid subscriberGuid)
        {

            _g2Service.G2DeleteSubscriberAsync(subscriberGuid);
            return StatusCode(202);
        }

    */

        #endregion

        #region Utility endpoints

        [HttpGet("invalid-email-domains")]
        [AllowAnonymous]
        public async Task<ActionResult<List<string>>> GetProhibitiedEmailDomains()
            => await _hiringManagerService.GetProhibitiedEmailDomains();

        #endregion Utility endpoints

        #region Candidate Search
        [HttpGet]
        [Authorize(Policy = "IsHiringManager")]
        [Route("candidates/query")]
        public async Task<IActionResult> SearchG2([FromQuery] CandidateSearchQueryDto searchDto)
        {
            var rVal = await _hiringManagerService.CandidateSearchByHiringManagerAsync(GetSubscriberGuid(), searchDto);
            return Ok(rVal);
        }
        #endregion
    }
}