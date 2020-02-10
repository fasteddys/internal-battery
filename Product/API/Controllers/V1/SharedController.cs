using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class SharedController : ControllerBase
    {
        private readonly IMapper _mapper;

        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger _syslog;

        public SharedController(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILogger<TopicController> sysLog)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _syslog=sysLog;
        }

        [HttpGet]
        [Route("api/country")]
        public async Task<IActionResult> GetCountries()
        {

            var countries = await _repositoryWrapper.Country.GetAllCountriesAsync();
              
            return Ok(_mapper.Map<List<CountryDto>>(countries));
        }

        [HttpGet]
        [Route("api/country/{countryGuid}/state")]
        public async Task<IActionResult> GetStatesByCountry(Guid countryGuid)
        {

            var states = await _repositoryWrapper.State.GetStatesByCountryGuid(countryGuid);

            return Ok(_mapper.Map<List<StateDto>>(states));
        }

        [HttpGet]
        [Route("api/state")]
        public async Task<IActionResult> GetStates()
        {
            var states = await _repositoryWrapper.State.GetStatesForDefaultCountry();

            return Ok(_mapper.Map<List<StateDto>>(states));
        }
    }
}
