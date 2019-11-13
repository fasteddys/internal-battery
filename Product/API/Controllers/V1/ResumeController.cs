using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using Hangfire;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Factory;
using AutoMapper.QueryableExtensions;
using System.Collections.Generic;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class ResumeController : Controller
    {
        private UpDiddyDbContext _db;
        private ISubscriberService _subscriberService;
        private IMapper _mapper;
        protected internal ILogger _syslog = null;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public ResumeController(UpDiddyDbContext db, ISubscriberService subscriberService, IMapper mapper, ILogger<ResumeController> sysLog, IRepositoryWrapper repositoryWrapper)
        {
            this._db = db;
            this._syslog = sysLog;
            this._subscriberService = subscriberService;
            this._mapper = mapper;
            this._repositoryWrapper = repositoryWrapper;
        }

        /// <summary>
        /// Resume Upload Endpoint that takes a resume upload and submits it to sovren to get HRXML and saves it in the
        /// subscriber profile staging store.
        /// </summary>
        /// <param name="resumeDto">The data transfer object which contains a subscriber guid and a base 64 encoded string representation of a resume</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile resume, bool parseResume = false)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // todo: research and implement a better way to handle soft deletes then manual checks everywhere
            Subscriber subscriber = _db.Subscriber
                .Include(s => s.SubscriberFile)
                .Where(s => s.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound(new BasicResponseDto { StatusCode = 404, Description = "Subscriber not found in the system." });

            await _subscriberService.AddResumeAsync(subscriber, resume, parseResume);

            int FileCount = subscriber.SubscriberFile.Count;
            SubscriberFileDto dto = _mapper.Map<SubscriberFileDto>(subscriber.SubscriberFile[FileCount > 0 ? FileCount - 1 : 0]);
            return Ok(dto);
        }

        [Authorize]
        [HttpPost]
        [Route("scan")]
        public async Task<IActionResult> Scan()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _subscriberService.QueueScanResumeJobAsync(subscriberGuid);
            return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Success!" });
        }



        [Authorize]
        [HttpPost]
        [Route("resolve-profile-merge/{resumeParseGuid}")]
        public async Task<IActionResult> ResolveProfileMerge([FromBody] string mergeInfo, Guid resumeParseGuid)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            ResumeParse resumeParse = await _repositoryWrapper.ResumeParseRepository.GetResumeParseByGuid(resumeParseGuid);

            if (resumeParse == null)
                return BadRequest(new BasicResponseDto() { StatusCode = 404, Description = $"ResumeParse {resumeParseGuid} not found!" });

            // Get subscriber 
            Subscriber subscriber = await SubscriberFactory.GetSubscriberById(_repositoryWrapper, resumeParse.SubscriberId);

            if (subscriberGuid != subscriber.SubscriberGuid)
                return BadRequest(new BasicResponseDto() { StatusCode = 401, Description = "Requester does not own resume parse" });


            await ResumeParseFactory.ResolveProfileMerge(_repositoryWrapper, _db, _mapper, _syslog, resumeParse, subscriber, mergeInfo);

            return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Success!" });
        }



        [Authorize]
        [HttpGet]
        [Route("profile-merge-questionnaire/{parseMergeGuid}")]
        public async Task<IActionResult> MergeInfo(Guid parseMergeGuid)
        {

            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            ResumeParse resumeParse = await _repositoryWrapper.ResumeParseRepository.GetResumeParseByGuid(parseMergeGuid);

            if (resumeParse == null )
                return BadRequest(new BasicResponseDto() { StatusCode = 404, Description = $"Resume parse {parseMergeGuid} does not exist" });

            Subscriber subscriber = await SubscriberFactory.GetSubscriberById(_repositoryWrapper, resumeParse.SubscriberId);

            if (subscriber == null)
                return BadRequest(new BasicResponseDto() { StatusCode = 404, Description = $"Subscriber   {resumeParse.SubscriberId} does not exist" });

            if (subscriberGuid != subscriber.SubscriberGuid)
                return BadRequest(new BasicResponseDto() { StatusCode = 401, Description = "Requester does not own resume parse" });
 
            ResumeParseQuestionnaireDto resumeParseQuestionaireDto = await ResumeParseFactory.GetResumeParseQuestionnaire(_repositoryWrapper, _mapper, resumeParse);

            return Ok(resumeParseQuestionaireDto);

        }

        /// <summary>
        /// Get any existing resume parses for user 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("resume-parse")]
        public async Task<IActionResult> ResumeParse(Guid guid)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
            if (subscriber == null)
            {
                return NotFound(new { code = 404, message = $"Subscriber {subscriberGuid} not found" });
            }

            if (subscriberGuid != subscriber.SubscriberGuid)
                return BadRequest(new BasicResponseDto() { StatusCode = 401, Description = "Requester does not own resume parse" });

            ResumeParse resumeParse = await _repositoryWrapper.ResumeParseRepository.GetLatestResumeParseRequiringMergeForSubscriber(subscriber.SubscriberId);

            if ( resumeParse != null )
                return Ok( _mapper.Map<ResumeParseDto>(resumeParse) );

            return NotFound(new { code = 404, message = $"Subscriber {subscriberGuid} not found" });
        }


    }
}