using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using UpDiddyLib.Helpers;
using System.Net.Http;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class ProfileController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected internal ISysLog _syslog = null;

        public ProfileController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = new SysLog(configuration, sysemail, serviceProvider);
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/Update")]
        public IActionResult Update([FromBody] SubscriberDto Subscriber)
        {

            try
            {
                Subscriber subscriber = _db.Subscriber
                     .Where(t => t.IsDeleted == 0 && t.SubscriberGuid.Equals(Subscriber.SubscriberGuid))
                     .FirstOrDefault();

                if (subscriber != null)
                {
                    if (!string.IsNullOrEmpty(Subscriber.FirstName))
                    {
                        subscriber.FirstName = Subscriber.FirstName;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.LastName))
                    {
                        subscriber.LastName = Subscriber.LastName;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.Address))
                    {
                        subscriber.Address = Subscriber.Address;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.City))
                    {
                        subscriber.City = Subscriber.City;
                    }
                    int stateId = 0;
                    if (Subscriber.SelectedState != Guid.Empty)
                    {
                        stateId = _db.State.Where(s => s.StateGuid.Value == Subscriber.SelectedState).Select(s => s.StateId).FirstOrDefault();
                    }
                    if (stateId != 0)
                    {
                        subscriber.StateId = stateId;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.PhoneNumber))
                    {
                        subscriber.PhoneNumber = Subscriber.PhoneNumber;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.FacebookUrl))
                    {
                        subscriber.FacebookUrl = Subscriber.FacebookUrl;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.TwitterUrl))
                    {
                        subscriber.TwitterUrl = Subscriber.TwitterUrl;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.LinkedInUrl))
                    {
                        subscriber.LinkedInUrl = Subscriber.LinkedInUrl;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.StackOverflowUrl))
                    {
                        subscriber.StackOverflowUrl = Subscriber.StackOverflowUrl;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.GithubUrl))
                    {
                        subscriber.GithubUrl = Subscriber.GithubUrl;
                    }
                }
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
            }
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
        [Route("api/[controller]/GetStatesByCountry/{countryGuid}")]
        public IActionResult GetStatesByCountry(Guid countryGuid)
        {
            var states = _db.State
                .Include(s => s.Country)
                .Where(s => s.IsDeleted == 0 && s.Country.CountryGuid == countryGuid)
                .OrderBy(s => s.Sequence)
                .ProjectTo<StateDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(states);
        }
    }
}