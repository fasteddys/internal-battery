using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly IStateService _stateService;

        public CountriesController(ICountryService countryService, IStateService stateService)
        {
            _countryService = countryService;
            _stateService = stateService;
        }

        [HttpGet]
        [Route("/V2/[controller]/{country}")]
        public async Task<IActionResult> GetCountryDetail(Guid country)
        {
            var countryDetail = await _countryService.GetCountryDetail(country);
            return Ok(countryDetail);
        }

        [HttpGet]
        [Route("/V2/[controller]/")]
        public async Task<IActionResult> GetCountries(int limit, int offset, string sort, string order)
        {
            var countries = await _countryService.GetAllCountries(limit, offset, sort, order);
            return Ok(countries);
        }

        [HttpPut]
        [Route("/V2/[controller]/{country}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCountry(Guid country, [FromBody]  CountryDetailDto countryDetailDto)
        {
            //TODO - Protect this endpoint
            await _countryService.UpdateCountry(country, countryDetailDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("/V2/[controller]/{country}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteCountry(Guid country)
        {
            //TODO - Protect this endpoint
            await _countryService.DeleteCountry(country);
            return StatusCode(204);
        }

        [HttpPost]
        [Route("/V2/[controller]/")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateCountry([FromBody] CountryDetailDto countryDetailDto)
        {
            await _countryService.CreateCountry(countryDetailDto);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("/V2/[controller]/states/{state}")]
        public async Task<IActionResult> GetStateDetail(Guid state)
        {
            var countryDetail = await _stateService.GetStateDetail(state);
            return Ok(countryDetail);
        }

        [HttpGet]
        [Route("/V2/[controller]/{country}/states")]
        public async Task<IActionResult> GetStates(Guid country, int limit, int offset, string sort, string order)
        {
            var states = await _stateService.GetAllStates(country, limit, offset, sort, order);
            return Ok(states);
        }

        [HttpPut]
        [Route("/V2/[controller]/{country}/states/{state}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]

        public async Task<IActionResult> UpdateState(Guid country, Guid state, [FromBody]  StateDetailDto stateDetailDto)
        {
            //TODO - Protect this endpoint
            await _stateService.UpdateState(country, state, stateDetailDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("/V2/[controller]/states/{state}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteState(Guid state)
        {
            //TODO - Protect this endpoint
            await _stateService.DeleteState(state);
            return StatusCode(204);
        }

        [HttpPost]
        [Route("/V2/[controller]/{country}/states/")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateState(Guid country, [FromBody] StateDetailDto StateDetailDto)
        {
            //TODO - Protect this endpoint
            await _stateService.CreateState(country, StateDetailDto);
            return StatusCode(201);
        }
    }
}
