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

        #region Business Logic

        
        
        
        #endregion
    }
}