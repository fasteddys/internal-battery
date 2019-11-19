using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
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
        [Route("{country}")]
        public async Task<IActionResult> GetCountryDetail(Guid country)
        {
            var countryDetail = await _countryService.GetCountryDetail(country);
            return Ok(countryDetail);
        }

        [HttpGet]
        public async Task<IActionResult> GetCountries(int limit, int offset, string sort, string order)
        {
            var countries = await _countryService.GetAllCountries(limit, offset, sort, order);
            return Ok(countries);
        }

        [HttpPut]
        [Route("{country}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCountry(Guid country, [FromBody]  CountryDetailDto countryDetailDto)
        {
            await _countryService.UpdateCountry(country, countryDetailDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("{country}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteCountry(Guid country)
        {
            await _countryService.DeleteCountry(country);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateCountry([FromBody] CountryDetailDto countryDetailDto)
        {
            await _countryService.CreateCountry(countryDetailDto);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("states/{state}")]
        public async Task<IActionResult> GetStateDetail(Guid state)
        {
            var countryDetail = await _stateService.GetStateDetail(state);
            return Ok(countryDetail);
        }

        [HttpGet]
        [Route("{country}/states")]
        public async Task<IActionResult> GetStates(Guid country, int limit, int offset, string sort, string order)
        {
            var states = await _stateService.GetAllStates(country, limit, offset, sort, order);
            return Ok(states);
        }

        [HttpPut]
        [Route("{country}/states/{state}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]

        public async Task<IActionResult> UpdateState(Guid country, Guid state, [FromBody]  StateDetailDto stateDetailDto)
        {
            await _stateService.UpdateState(country, state, stateDetailDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("states/{state}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteState(Guid state)
        {
            await _stateService.DeleteState(state);
            return StatusCode(204);
        }

        [HttpPost]
        [Route("{country}/states/")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateState(Guid country, [FromBody] StateDetailDto StateDetailDto)
        {
            await _stateService.CreateState(country, StateDetailDto);
            return StatusCode(201);
        }
    }
}
