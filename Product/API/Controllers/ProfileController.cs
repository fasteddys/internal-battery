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
        public IActionResult NewSubscriber(string SubscriberGuid, string SubscriberEmail)
        {
            Subscriber subscriber = new Subscriber();
            subscriber.SubscriberGuid = Guid.Parse(SubscriberGuid);
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
            Subscriber subscriber = _db.Subscriber
                 .Where(t => t.IsDeleted == 0 && t.SubscriberGuid.Equals(Subscriber.SubscriberGuid))
                 // use .SelectMany for Skills?
                 .FirstOrDefault();

            if (subscriber != null)
            {
                subscriber.FirstName = Subscriber.FirstName;
                subscriber.LastName = Subscriber.LastName;
                subscriber.Address = Subscriber.Address;
                subscriber.City = Subscriber.City;
                int stateId = 0;
                if (Subscriber.State.StateGuid.HasValue)
                    stateId = _db.State.Where(s => s.StateGuid.Value == Subscriber.State.StateGuid.Value).Select(s => s.StateId).FirstOrDefault();
                else
                    subscriber.StateId = null;
                subscriber.PhoneNumber = Subscriber.PhoneNumber;
                subscriber.FacebookUrl = Subscriber.FacebookUrl;
                subscriber.TwitterUrl = Subscriber.TwitterUrl;
                subscriber.LinkedInUrl = Subscriber.LinkedInUrl;
                subscriber.StackOverflowUrl = Subscriber.StackOverflowUrl;
                subscriber.GithubUrl = Subscriber.GithubUrl;
            }
            
            // overwrite anything that currently exists for skills...
            var updatedSkills = _db.Skill
                .Where(s => s.IsDeleted == 0)
                .Join(Subscriber.Skills, e => e.SkillGuid, n => n.SkillGuid, (e, n) => new { e, n })
                .ToList();

            foreach(var skill in Subscriber.Skills)
            {
                _db.SubscriberSkill.Add(new SubscriberSkill()
                {
                     
                });
            }

            // todo: identify overlap, update existing skills and update them with a new modify date
            //var existingSkills = _db.SubscriberSkill
            //    .Where(sk => sk.IsDeleted == 0 && sk.SubscriberId == subscriber.SubscriberId)
            //    .Join(updatedSkills, sk => sk.SkillId, us => us.SkillId, (sk, us) => sk)
            //    .ToList();

            //foreach (var skill in existingSkills)
            //{
            //    skill.ModifyDate = DateTime.UtcNow;
            //}
            // , add anything that is new
            //var newSkills = updatedSkills
            //    .GroupJoin(
            //        _db.SubscriberSkill,
            //        ns => ns.SkillId,
            //        ss => ss.SkillId, (ns, ss) => new { ns, ss = ss.DefaultIfEmpty() })
            //    .SelectMany(x =>
            //    x.ss.Select(t2 => new { t1 = x.ns, t2 = t2 }));


            // , remove anything that doesn't still exist


            _db.SaveChanges();

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
                .Join(_db.SubscriberSkill, s => s.SubscriberId, sk => sk.SubscriberId, (s, sk) => new { sk.SkillId })
                .Join(_db.Skill, x => x.SkillId, s => s.SkillId, (x, s) => s)
                .Distinct()
                .OrderBy(s => s.SkillName)
                .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(subscriberSkills);
        }

    }
}