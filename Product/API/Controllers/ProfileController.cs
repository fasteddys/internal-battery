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
        [Route("api/[controller]/{SubscriberGuid}")]
        public IActionResult Get(Guid SubscriberGuid)
        {
            Subscriber subscriber = _db.Subscriber
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .Where(t => t.IsDeleted == 0 && t.SubscriberGuid == SubscriberGuid)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound();

            return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]/CreateSubscriber/{SubscriberGuid}/{SubscriberEmail}")]
        public IActionResult NewSubscriber(Guid SubscriberGuid, string SubscriberEmail)
        {
            Subscriber subscriber = new Subscriber();
            subscriber.SubscriberGuid = SubscriberGuid;
            subscriber.Email = SubscriberEmail;
            subscriber.CreateDate = DateTime.Now;
            subscriber.ModifyDate = DateTime.Now;
            subscriber.IsDeleted = 0;
            subscriber.ModifyGuid = Guid.NewGuid();
            subscriber.CreateGuid = Guid.NewGuid();

            // Save subscriber to database 
            _db.Subscriber.Add(subscriber);
            _db.SaveChanges();

            return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/Update")]
        public IActionResult Update([FromBody] SubscriberDto Subscriber)
        {
            var subscriberGuid = new SqlParameter("@SubscriberGuid", Subscriber.SubscriberGuid);
            var firstName = new SqlParameter("@FirstName", (object)Subscriber.FirstName ?? DBNull.Value);
            var lastName = new SqlParameter("@LastName", (object)Subscriber.LastName ?? DBNull.Value);
            var address = new SqlParameter("@Address", (object)Subscriber.Address ?? DBNull.Value);
            var city = new SqlParameter("@City", (object)Subscriber.City ?? DBNull.Value);
            var stateGuid = new SqlParameter("@StateGuid", (object)Subscriber.State.StateGuid ?? DBNull.Value);
            var phoneNumber = new SqlParameter("@PhoneNumber", (object)Subscriber.PhoneNumber ?? DBNull.Value);
            var facebookUrl = new SqlParameter("@FacebookUrl", (object)Subscriber.FacebookUrl ?? DBNull.Value);
            var twitterUrl = new SqlParameter("@TwitterUrl", (object)Subscriber.TwitterUrl ?? DBNull.Value);
            var linkedInUrl = new SqlParameter("@LinkedInUrl", (object)Subscriber.LinkedInUrl ?? DBNull.Value);
            var stackOverflowUrl = new SqlParameter("@StackOverflowUrl", (object)Subscriber.StackOverflowUrl ?? DBNull.Value);
            var gitHubUrl = new SqlParameter("@GitHubUrl", (object)Subscriber.GithubUrl ?? DBNull.Value);

            DataTable table = new DataTable();
            table.Columns.Add("Guid", typeof(Guid));
            if (Subscriber.Skills != null)
            {
                foreach (var skill in Subscriber.Skills)
                {
                    table.Rows.Add(skill.SkillGuid);
                }
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

        // should we have a "utility" or "shared" API controller for things like this?
        [HttpGet]
        [Route("api/[controller]/GetCountries")]
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
        [Route("api/[controller]/GetStatesByCountry/{countryGuid?}")]
        public IActionResult GetStatesByCountry(Guid? countryGuid = null)
        {
            IQueryable<State> states;

            if (!countryGuid.HasValue)
            {
                // if country is not specified, retrieve the first country according to sequence (USA! USA!)
                states = _db.State
                    .Include(s => s.Country)
                    .Where(s => s.IsDeleted == 0 && s.Country.Sequence == 1);
            }
            else
            {
                states = _db.State
                    .Include(s => s.Country)
                    .Where(s => s.IsDeleted == 0 && s.Country.CountryGuid == countryGuid);
            }

            return Ok(states.OrderBy(s => s.Sequence).ProjectTo<StateDto>(_mapper.ConfigurationProvider));
        }

        [HttpGet]
        [Route("api/[controller]/GetSkills/{userQuery}")]
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
        [Route("api/[controller]/GetSkillsBySubscriber/{subscriberGuid}")]
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
    }
}