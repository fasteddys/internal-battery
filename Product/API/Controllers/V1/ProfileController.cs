using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Web;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Workflow;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;

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
        private readonly ICloudTalentService _cloudTalentService;
        private readonly ISubscriberService _subscriberService = null;
        private readonly IHangfireService _hangfireService;

        public ProfileController(UpDiddyDbContext db
        , IMapper mapper
        , Microsoft.Extensions.Configuration.IConfiguration configuration
        , ILogger<ProfileController> sysLog
        , IHttpClientFactory httpClientFactory
        , ISubscriberService subscriberService
        , IRepositoryWrapper repositoryWrapper
        , IHangfireService hangfireService
        , ICloudTalentService cloudTalentService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;     
            _subscriberService = subscriberService;
            _hangfireService = hangfireService;
            _cloudTalentService = cloudTalentService;
        }

        #region profile tenants

        /// <summary>
        /// search profiles 
        /// </summary>
        /// <param name="profileQueryDto"></param>
        /// <returns></returns>
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [HttpGet]
        [Route("api/[controller]/tenants")]
        public IActionResult TenantList()
        {
            // search profiles 
            BasicResponseDto rVal = _cloudTalentService.ProfileTenantList();
            return Ok(rVal);
        }


        #endregion

        #region Profile look-up data 

        [HttpGet]
        [Route("api/skill/{userQuery}")]
        public async Task<IActionResult> GetSkills(string userQuery)
        {
            userQuery = HttpUtility.UrlDecode(userQuery);

            var skills = await _db.Skill
                .Where(s => s.IsDeleted == 0 && s.SkillName.Contains(userQuery))
                .OrderBy(s => s.SkillName)
                .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(skills);
        }

        [HttpGet]
        [Route("api/company/{userQuery}")]
        public async Task<IActionResult> GetCompanies(string userQuery)
        {
            var companies = await _db.Company
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
                .ToListAsync();

            return Ok(companies);
        }

        [HttpGet]
        [Route("api/educational-institution/{userQuery}")]
        public async Task<IActionResult> GetEducationalInstitutions(string userQuery)
        {
            var educationalInstitutions = await _db.EducationalInstitution
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
                .ToListAsync();

            return Ok(educationalInstitutions);
        }

        [HttpGet]
        [Route("api/educational-degree/{userQuery}")]
        public async Task<IActionResult> GetEducationalDegrees(string userQuery)
        {
            var educationalDegrees = await _db.EducationalDegree
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
                .ToListAsync();

            return Ok(educationalDegrees);
        }

        [Route("api/educational-degree-types")]
        public async Task<IActionResult> GetEducationalDegreesTypes()
        {
            var educationalDegreesType = await _db.EducationalDegreeType
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.DegreeType)
                .ProjectTo<EducationalDegreeTypeDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(educationalDegreesType);
        }

        [HttpGet]
        [Route("api/compensation-types")]
        public async Task<IActionResult> GetCompensationTypes()
        {
            var compensationTypes = await _db.CompensationType
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.CompensationTypeName)
                .ProjectTo<CompensationTypeDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

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
        public IActionResult ProfileSearch([FromBody] ProfileQueryDto profileQueryDto)
        {
            // search profiles 
            ProfileSearchResultDto rVal = _cloudTalentService.ProfileSearch(profileQueryDto);
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
        public IActionResult ProfileAdd(Guid SubscriberGuid)
        {
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(SubscriberGuid));
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
        public IActionResult ProfileDelete(Guid SubscriberGuid)
        {
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentDeleteProfile(SubscriberGuid, null));
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
        public IActionResult DeleteProfileByGoogleName([FromBody] string TalentCloudUri)
        {
            _cloudTalentService.DeleteProfileFromCloudTalentByUri(TalentCloudUri);
            return Ok();
        }
 


  

        #endregion

    }
}