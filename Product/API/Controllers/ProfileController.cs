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
using UpDiddyApi.Business.Factory;
using UpDiddy.Helpers;

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

            return Ok(_mapper.Map<SubscriberDto>(subscriber));
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

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/AddWorkHistory")]
        // TODO looking into consolidating Add and Update to reduce code redundancy
        public IActionResult AddWorkHistory([FromBody] SubscriberWorkHistoryDto WorkHistoryDto)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);    
            Company company = CompanyFactory.GetOrAdd(_db, WorkHistoryDto.Company);
            int companyId = company != null ? company.CompanyId : -1;
            CompensationType compensationType = CompensationTypeFactory.GetCompensationTypeByName(_db, WorkHistoryDto.CompensationType);
            int compensationTypeId = 0;
            if (compensationType != null)
                compensationTypeId = compensationType.CompensationTypeId;
            else
            {
                compensationType = CompensationTypeFactory.GetOrAdd(_db, Constants.NotSpecifedOption);
                compensationTypeId = compensationType.CompensationTypeId;
            }

            if (subscriber == null)
                return BadRequest();

            SubscriberWorkHistory WorkHistory = new SubscriberWorkHistory()
            {
                SubscriberWorkHistoryGuid = Guid.NewGuid(),
                CreateGuid = Guid.NewGuid(),
                ModifyGuid = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                IsDeleted = 0,                
                SubscriberId = subscriber.SubscriberId,
                StartDate = WorkHistoryDto.StartDate,
                EndDate = WorkHistoryDto.EndDate,
                IsCurrent = WorkHistoryDto.IsCurrent,
                Title = WorkHistoryDto.Title,
                JobDecription = WorkHistoryDto.JobDecription,
                Compensation = WorkHistoryDto.Compensation,
                CompensationTypeId = compensationTypeId,
                CompanyId = companyId
            };

            _db.SubscriberWorkHistory.Add(WorkHistory);
            _db.SaveChanges();
            return Ok(_mapper.Map<SubscriberWorkHistoryDto>(WorkHistory));
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/UpdateWorkHistory")]
        public IActionResult UpdateWorkHistory([FromBody] SubscriberWorkHistoryDto WorkHistoryDto)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            Company company = CompanyFactory.GetOrAdd(_db, WorkHistoryDto.Company);
            int companyId = company != null ? company.CompanyId : -1;
            CompensationType compensationType = CompensationTypeFactory.GetCompensationTypeByName(_db, WorkHistoryDto.CompensationType);
            int compensationTypeId = 0;
            if (compensationType != null)
                compensationTypeId = compensationType.CompensationTypeId;
            else
            {
                compensationType = CompensationTypeFactory.GetOrAdd(_db,Constants.NotSpecifedOption);
                compensationTypeId = compensationType.CompensationTypeId;
            }
       
            if (subscriber == null)
                return BadRequest();

            SubscriberWorkHistory WorkHistory = SubscriberWorkHistoryFactory.GetWorkHistoryByGuid(_db, WorkHistoryDto.SubscriberWorkHistoryGuid);
            if  (WorkHistory == null )
                return BadRequest();

            // Update the company ID
            WorkHistory.CompanyId = companyId;
            WorkHistory.StartDate = WorkHistoryDto.StartDate;
            WorkHistory.EndDate = WorkHistoryDto.EndDate;
            WorkHistory.JobDecription = WorkHistoryDto.JobDecription;
            WorkHistory.Title = WorkHistoryDto.Title;
            WorkHistory.IsCurrent = WorkHistoryDto.IsCurrent;
            WorkHistory.Compensation = WorkHistoryDto.Compensation;
            WorkHistory.CompensationTypeId = compensationTypeId;            
            _db.SaveChanges();
            return Ok(_mapper.Map<SubscriberWorkHistoryDto>(WorkHistory));
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



        // TODO find a better home for these lookup endpoints - maybe a new lookup or data endpoint?
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

        [HttpGet]
        [Route("api/company/{userQuery}")]
        public IActionResult GetCompanies(string userQuery)
        {
            var companies = _db.Company
                .Where(c => c.IsDeleted == 0 && c.CompanyName.Contains(userQuery))
                .OrderBy(c => c.CompanyName)
                .ProjectTo<CompanyDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(companies);
        }


        [HttpGet]
        [Route("api/compensation-types")]
        public IActionResult GetCompensationTypes()
        {
            var compensationTypes = _db.CompensationType
                .Where(c => c.IsDeleted == 0  )
                .OrderBy(c => c.CompensationTypeName)
                .ProjectTo<CompensationTypeDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(compensationTypes);
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

         
        [Authorize]
        [HttpGet]
        [Route("api/[controller]/GetWorkHistory")]
        public IActionResult GetWorkHistory()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            if (subscriber == null)
                return BadRequest();
 
            var workHistory = _db.SubscriberWorkHistory
            .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriber.SubscriberId)
            .OrderByDescending(s => s.StartDate)
            .ProjectTo<SubscriberWorkHistoryDto>(_mapper.ConfigurationProvider)
            .ToList();

            return Ok(workHistory);
        }



    }
}