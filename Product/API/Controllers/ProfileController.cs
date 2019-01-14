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

        public ProfileController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
        }

        [HttpGet]
        [Authorize]
        [Route("api/[controller]")]
        public IActionResult Get()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            Subscriber subscriber = _db.Subscriber
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .Where(t => t.IsDeleted == 0 && t.SubscriberGuid == subscriberGuid)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound();

            SubscriberDto subscriberDto = _mapper.Map<SubscriberDto>(subscriber);
            subscriberDto.EducationHistory = RetrieveEducationHistory(subscriber.SubscriberId);
            subscriberDto.WorkHistory = RetrieveWorkHistory(subscriber.SubscriberId);

            return Ok(subscriberDto);
        }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]")]
        public IActionResult NewSubscriber()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Subscriber subscriber = _db.Subscriber.Where(t => t.IsDeleted == 0 && t.SubscriberGuid == subscriberGuid).FirstOrDefault();

            // Subscriber exists do NOT create a duplicate
            if (subscriber != null)
                return BadRequest(new { code = 400, message = "Subscriber is already in the system" });

            subscriber = new Subscriber();
            subscriber.SubscriberGuid = subscriberGuid;
            subscriber.Email = HttpContext.User.FindFirst("emails").Value;
            subscriber.CreateDate = DateTime.Now;
            subscriber.ModifyDate = DateTime.Now;
            subscriber.IsDeleted = 0;
            subscriber.ModifyGuid = subscriberGuid;
            subscriber.CreateGuid = subscriberGuid;

            // Save subscriber to database 
            _db.Subscriber.Add(subscriber);
            _db.SaveChanges();

            return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }

        [Authorize]
        [HttpPut]
        [Route("api/[controller]")]
        public IActionResult Update([FromBody] SubscriberDto Subscriber)
        {
            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subsriberGuidClaim != Subscriber.SubscriberGuid)
                return Unauthorized();

            Subscriber subscriberFromDb = _db.Subscriber.Where(t => t.IsDeleted == 0 && t.SubscriberGuid == Subscriber.SubscriberGuid).FirstOrDefault();

            var subscriberGuid = new SqlParameter("@SubscriberGuid", Subscriber.SubscriberGuid);
            var firstName = new SqlParameter("@FirstName", (object)(Subscriber.FirstName ?? subscriberFromDb.FirstName) ?? DBNull.Value);
            var lastName = new SqlParameter("@LastName", (object)(Subscriber.LastName ?? subscriberFromDb.LastName) ?? DBNull.Value);
            var address = new SqlParameter("@Address", (object)(Subscriber.Address ?? subscriberFromDb.Address) ?? DBNull.Value);
            var city = new SqlParameter("@City", (object)(Subscriber.City ?? subscriberFromDb.City) ?? DBNull.Value);
            var stateGuid = new SqlParameter("@StateGuid", (Subscriber?.State?.StateGuid != null ? (object)Subscriber.State.StateGuid : (subscriberFromDb.State?.StateGuid != null ? (object)subscriberFromDb.State.StateGuid : DBNull.Value)));
            var phoneNumber = new SqlParameter("@PhoneNumber", (object)(Subscriber.PhoneNumber ?? subscriberFromDb.PhoneNumber) ?? DBNull.Value);
            var facebookUrl = new SqlParameter("@FacebookUrl", (object)Subscriber.FacebookUrl ?? DBNull.Value);
            var twitterUrl = new SqlParameter("@TwitterUrl", (object)Subscriber.TwitterUrl ?? DBNull.Value);
            var linkedInUrl = new SqlParameter("@LinkedInUrl", (object)Subscriber.LinkedInUrl ?? DBNull.Value);
            var stackOverflowUrl = new SqlParameter("@StackOverflowUrl", (object)Subscriber.StackOverflowUrl ?? DBNull.Value);
            var gitHubUrl = new SqlParameter("@GitHubUrl", (object)Subscriber.GithubUrl ?? DBNull.Value);

            // todo: update stored procedure to be additive so that we don't need to get current subscriber's skills here.
            var subscriberSkills = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == Subscriber.SubscriberGuid)
                .Join(_db.SubscriberSkill.Where(ss => ss.IsDeleted == 0), s => s.SubscriberId, sk => sk.SubscriberId, (s, sk) => new { sk.SkillId })
                .Join(_db.Skill.Where(s => s.IsDeleted == 0), x => x.SkillId, s => s.SkillId, (x, s) => s)
                .Distinct()
                .OrderBy(s => s.SkillName)
                .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                .ToList();

            DataTable table = new DataTable();
            table.Columns.Add("Guid", typeof(Guid));
            if (Subscriber.Skills != null)
            {
                foreach (var skill in Subscriber.Skills)
                {
                    table.Rows.Add(skill.SkillGuid);
                }
            }
            foreach (var skill in subscriberSkills)
            {
                table.Rows.Add(skill.SkillGuid);
            }

            var skillGuids = new SqlParameter("@SkillGuids", table);
            skillGuids.SqlDbType = SqlDbType.Structured;
            skillGuids.TypeName = "dbo.GuidList";

            var spParams = new object[] { subscriberGuid, firstName, lastName, address, city, stateGuid, phoneNumber, facebookUrl, twitterUrl, linkedInUrl, stackOverflowUrl, gitHubUrl, skillGuids };

            var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Update_Subscriber] 
                    @SubscriberGuid,
                    @FirstName,
	                @LastName,
	                @Address,
	                @City,
	                @StateGuid,
                    @PhoneNumber,
	                @FacebookUrl,
	                @TwitterUrl,
	                @LinkedInUrl,
	                @StackOverflowUrl,
	                @GithubUrl,
	                @SkillGuids", spParams);

            return Ok();
        }

        [HttpPut]
        [Route("api/[controller]/onboard/{SubscriberGuid}")]
        public IActionResult Onboard(Guid SubscriberGuid)
        {
            if(SubscriberGuid == null || SubscriberGuid == Guid.Empty)
                return BadRequest(new { code = 400, message = "No Subscriber found in system." });

            Subscriber subscriber = _db.Subscriber.Where(t => t.IsDeleted == 0 && t.SubscriberGuid == SubscriberGuid).FirstOrDefault();

            subscriber.HasOnboarded = 1;
            _db.Subscriber.Update(subscriber);
            _db.SaveChanges();

            return Ok();
        }

        // should we have a "utility" or "shared" API controller for things like this?
        [HttpGet]
        [Route("api/country")]
        public IActionResult GetCountries()
        {
            var countries = _db.Country
                .Join(_db.State, c => c.CountryId, s => s.CountryId, (c, s) => c)
                .Distinct()
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.Sequence)
                .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(countries);
        }

        [HttpGet]
        [Route("api/country/{countryGuid}/state")]
        public IActionResult GetStatesByCountry(Guid countryGuid)
        {
            IQueryable<State> states;

            states = _db.State
                .Include(s => s.Country)
                .Where(s => s.IsDeleted == 0 && s.Country.CountryGuid == countryGuid);

            return Ok(states.OrderBy(s => s.Sequence).ProjectTo<StateDto>(_mapper.ConfigurationProvider));
        }

        [HttpGet]
        [Route("api/state")]
        public IActionResult GetStates()
        {
            IQueryable<State> states;
            states = _db.State
                .Include(s => s.Country)
                .Where(s => s.IsDeleted == 0 && s.Country.Sequence == 1);

            return Ok(states.OrderBy(s => s.Sequence).ProjectTo<StateDto>(_mapper.ConfigurationProvider));
        }

        [HttpGet]
        [Route("api/skill/{userQuery}")]
        public IActionResult GetSkills(string userQuery)
        {
            var skills = _db.Skill
                .Where(s => s.IsDeleted == 0 && s.SkillName.Contains(userQuery))
                .OrderBy(s => s.SkillName)
                .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(skills);
        }

        [Authorize]
        [HttpGet]
        [Route("api/[controller]/{subscriberGuid}/skill")]
        public IActionResult GetSkillsBySubscriber(Guid subscriberGuid)
        {
            var subscriberSkills = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid.Value == subscriberGuid)
                .Join(_db.SubscriberSkill.Where(ss => ss.IsDeleted == 0), s => s.SubscriberId, sk => sk.SubscriberId, (s, sk) => new { sk.SkillId })
                .Join(_db.Skill.Where(s => s.IsDeleted == 0), x => x.SkillId, s => s.SkillId, (x, s) => s)
                .Distinct()
                .OrderBy(s => s.SkillName)
                .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(subscriberSkills);
        }

        #region Business Logic

        
        private IList<SubscriberEducationHistoryDto> RetrieveEducationHistory(int subscriberId)
        {
            IList<SubscriberEducationHistoryDto> educationHistory = _db.SubscriberEducationHistory
                    .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                    .ProjectTo<SubscriberEducationHistoryDto>(_mapper.ConfigurationProvider)
                    .ToList();

            // If the subscriber has no education history, return an empty list.
            if(educationHistory.Count == 0)
            {
                return new List<SubscriberEducationHistoryDto>();
            }

            try
            {
                foreach (SubscriberEducationHistoryDto sub in educationHistory)
                {
                    EducationalInstitutionDto instituation = _db.EducationalInstitution
                        .Where(ei => ei.IsDeleted == 0 && ei.EducationalInstitutionId == Int32.Parse(sub.EducationalInstitutionId))
                        .ProjectTo<EducationalInstitutionDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefault();
                    sub.EducationalInstitution = instituation.Name;
                }
            }
            catch(Exception e)
            {
                _syslog.Log(LogLevel.Information, "ProfileController:RetrieveEducationHistory - No educational institution found.");
            }

            try
            {
                foreach (SubscriberEducationHistoryDto sub in educationHistory)
                {

                    EducationalDegreeDto degree = _db.EducationalDegree
                        .Where(d => d.IsDeleted == 0 && d.EducationalDegreeId == sub.EducationalDegreeId)
                        .ProjectTo<EducationalDegreeDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefault();
                    sub.EducationalDegree = degree.Degree;
                }
            }
            catch(Exception e)
            {
                _syslog.Log(LogLevel.Information, "ProfileController:RetrieveEducationHistory - No educational degree found.");
            }

            try
            {
                foreach (SubscriberEducationHistoryDto sub in educationHistory)
                {

                    EducationalDegreeTypeDto degreeType = _db.EducationalDegreeType
                        .Where(dt => dt.IsDeleted == 0 && dt.EducationalDegreeTypeId == sub.EducationalDegreeTypeId)
                        .ProjectTo<EducationalDegreeTypeDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefault();
                    sub.EducationalDegreeType = degreeType.DegreeType;
                }
            }
            catch(Exception e)
            {
                _syslog.Log(LogLevel.Information, "ProfileController:RetrieveEducationHistory - No educational degree type found.");
            }


            return educationHistory;
        }

        private IList<SubscriberWorkHistoryDto> RetrieveWorkHistory(int subscriberId)
        {
            IList<SubscriberWorkHistoryDto> workHistory = _db.SubscriberWorkHistory
                    .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                    .ProjectTo<SubscriberWorkHistoryDto>(_mapper.ConfigurationProvider)
                    .ToList();

            // If the subscriber has no education history, return an empty list.
            if (workHistory.Count == 0)
            {
                return new List<SubscriberWorkHistoryDto>();
            }

            try
            {
                foreach (SubscriberWorkHistoryDto sub in workHistory)
                {
                    CompanyDto company = _db.Company
                        .Where(c => c.IsDeleted == 0 && c.CompanyId == sub.CompanyId)
                        .ProjectTo<CompanyDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefault();
                    sub.Company = company.CompanyName;
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, "ProfileController:RetrieveWorkHistory - No company found.");
            }

            try
            {
                foreach (SubscriberWorkHistoryDto sub in workHistory)
                {

                    CompensationTypeDto compensationType = _db.CommunicationType
                    .Where(ct => ct.IsDeleted == 0 && ct.CommunicationTypeId == sub.CompensationTypeId)
                    .ProjectTo<CompensationTypeDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefault();
                    sub.CompensationType = compensationType.CompensationTypeName;
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, "ProfileController:RetrieveWorkHistory - No compensation type found.");
            }

            return workHistory;
        }
        
        #endregion
    }
}