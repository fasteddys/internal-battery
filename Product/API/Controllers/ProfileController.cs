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

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class ProfileController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        //private readonly CCQueue _queue = null;
        public ProfileController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
        }

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
                    if(Subscriber.StateId != 0)
                    {
                        subscriber.StateId = Subscriber.StateId;
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

        [HttpGet]
        [Route("api/[controller]/GetCountries")]
        public IActionResult GetCountries()
        {
            var countries = _db.Country
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

        [HttpGet]
        [Route("api/[controller]/LocationList")]
        public IActionResult LocationList()
        {
            

            IList<CountryStateDto> CountryStates = (
                from state in _db.State
                join country in _db.Country on state.CountryId equals country.CountryId
                select new
                {
                    country.DisplayName,
                    country.Code2,
                    country.Code3,
                    state.Name,
                    state.Code,
                    state.StateId
                }).ProjectTo<CountryStateDto>(_mapper.ConfigurationProvider).ToList();

            return Ok(CountryStates);

        }
    }

}