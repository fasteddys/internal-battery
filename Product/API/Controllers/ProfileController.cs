using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyLib.Helpers;
using System.Web;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Workflow;
using Hangfire;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class ProfileController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected internal ILogger _syslog = null;
        private readonly CloudTalent _cloudTalent = null;
        private readonly ISubscriberService _subscriberService = null;

        public ProfileController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog, IHttpClientFactory httpClientFactory, ISubscriberService subscriberService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, httpClientFactory);
            _subscriberService = subscriberService;
        }

        #region Profile look-up data 

        [HttpGet]
        [Route("api/skill/{userQuery}")]
        public IActionResult GetSkills(string userQuery)
        {
            userQuery = HttpUtility.UrlDecode(userQuery);

            var skills = _db.Skill
                .Where(s => s.IsDeleted == 0 && s.SkillName.Contains(userQuery))
                .OrderBy(s => s.SkillName)
                .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(skills);
        }

        [HttpGet]
        [Route("api/company/{userQuery}")]
        public IActionResult GetCompanies(string userQuery)
        {
            var companies = _db.Company
                .Where(c => c.IsDeleted == 0 && c.CompanyName.Contains(userQuery))
                .OrderBy(c => c.CompanyName)
                .Select(c => new Company()
                  {
                      CompanyName = HttpUtility.HtmlDecode(c.CompanyName),
                      CompanyGuid = c.CompanyGuid,
                      CompanyId = c.CompanyId,
                      CreateDate = c.CreateDate,
                      CreateGuid = c.CreateGuid,
                      IsDeleted = c.IsDeleted,
                      ModifyDate = c.ModifyDate,
                      ModifyGuid = c.ModifyGuid
                  })
                .ProjectTo<CompanyDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(companies);
        }

        [HttpGet]
        [Route("api/educational-institution/{userQuery}")]
        public IActionResult GetEducationalInstitutions(string userQuery)
        {
            var educationalInstitutions = _db.EducationalInstitution
                .Where(c => c.IsDeleted == 0 && c.Name.Contains(userQuery))
                .OrderBy(c => c.Name)
                .Select(ei => new EducationalInstitution()
                {
                    Name = HttpUtility.HtmlDecode(ei.Name),
                    EducationalInstitutionGuid = ei.EducationalInstitutionGuid,
                    CreateDate = ei.CreateDate,
                    CreateGuid = ei.CreateGuid,
                    EducationalInstitutionId = ei.EducationalInstitutionId,
                    IsDeleted = ei.IsDeleted,
                    ModifyDate = ei.ModifyDate,
                    ModifyGuid = ei.ModifyGuid
                })
                .ProjectTo<EducationalInstitutionDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(educationalInstitutions);
        }

        [HttpGet]
        [Route("api/educational-degree/{userQuery}")]
        public IActionResult GetEducationalDegrees(string userQuery)
        {
            var educationalDegrees = _db.EducationalDegree
                .Where(c => c.IsDeleted == 0 && c.Degree.Contains(userQuery))
                .OrderBy(c => c.Degree)
                .Select(ed => new EducationalDegree()
                {
                    Degree = HttpUtility.HtmlDecode(ed.Degree),
                    EducationalDegreeGuid = ed.EducationalDegreeGuid,
                    CreateDate = ed.CreateDate,
                    CreateGuid = ed.CreateGuid,
                    EducationalDegreeId = ed.EducationalDegreeId,
                    IsDeleted = ed.IsDeleted,
                    ModifyDate = ed.ModifyDate,
                    ModifyGuid = ed.ModifyGuid
                })
                .ProjectTo<EducationalDegreeDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(educationalDegrees);
        }

        [Route("api/educational-degree-types")]
        public IActionResult GetEducationalDegreesTypes()
        {
            var educationalDegreesType = _db.EducationalDegreeType
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.DegreeType)
                .ProjectTo<EducationalDegreeTypeDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(educationalDegreesType);
        }

        [HttpGet]
        [Route("api/compensation-types")]
        public IActionResult GetCompensationTypes()
        {
            var compensationTypes = _db.CompensationType
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.CompensationTypeName)
                .ProjectTo<CompensationTypeDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(compensationTypes);
        }

        #endregion

        #region Profile Queries


        /// <summary>
        /// search profiles 
        /// </summary>
        /// <param name="profileQueryDto"></param>
        /// <returns></returns>
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [HttpGet]
        [Route("api/[controller]/{PageNum?}")]
        public async Task<IActionResult> ProfileSearch([FromBody] ProfileQueryDto profileQueryDto)
        {
            // search profiles 
            ProfileSearchResultDto rVal = _cloudTalent.ProfileSearch(profileQueryDto);
            return Ok(rVal);
        }


        /// <summary>
        /// add a subscriber to the google cloud 
        /// </summary>
        /// <param name="SubscriberGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [HttpPost]
        [Route("api/[controller]/{SubscriberGuid}")]
        public async Task<IActionResult> ProfileAdd(Guid SubscriberGuid)
        {
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(SubscriberGuid));
            return Ok( );
        }


        /// <summary>
        /// Delete the user's profile from the google cloud using their subscriber id 
        /// </summary>
        /// <param name="SubscriberGuid"></param>
        /// <returns></returns>

        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [HttpDelete]
        [Route("api/[controller]/{SubscriberGuid}")]
        public async Task<IActionResult> ProfileDelete(Guid SubscriberGuid)
        {
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentDeleteProfile(SubscriberGuid));
            return Ok();
        }


    
        /// <summary>
        /// Delete the user's profile from the google cloud using their google cloud uri 
        /// </summary>
        /// <param name="SubscriberGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [HttpDelete]
        [Route("api/[controller]/delete-by-uri")]
        public async Task<IActionResult> DeleteProfileByGoogleName([FromBody] string TalentCloudUri)
        {
            _cloudTalent.DeleteProfileFromCloudTalentByUri(_db, TalentCloudUri);
            return Ok();
        }
 


        /// <summary>
        /// Delete the user's profile from the google cloud using their google cloud uri 
        /// </summary>
        /// <param name="SubscriberGuid"></param>
        /// <returns></returns>
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpPut]
        [Route("api/[controller]/force-reindex")]
        public async Task<IActionResult> ReindexAll( )
        {
            _subscriberService.MarkSubscribersForReindex();
            return Ok();
        }



        #endregion

    }
}