using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
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
        [Route("{country:guid}")]
        public async Task<IActionResult> GetCountryDetail(Guid country)
        {
            var countryDetail = await _countryService.GetCountryDetail(country);
            return Ok(countryDetail);
        }

        [HttpGet]
        public async Task<IActionResult> GetCountries(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var countries = await _countryService.GetAllCountries(limit, offset, sort, order);
            return Ok(countries);
        }

        [HttpPut]
        [Route("{country:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCountry(Guid country, [FromBody]  CountryDetailDto countryDetailDto)
        {
            await _countryService.UpdateCountry(country, countryDetailDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{country:guid}")]
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
        [Route("{country:guid}/states/{state:guid}")]
        public async Task<IActionResult> GetStateDetail(Guid state)
        {
            var stateDetail = await _stateService.GetStateDetail(state);
            return Ok(stateDetail);
        }

        [HttpGet]
        [Route("{country:guid}/states")]
        public async Task<IActionResult> GetStates(Guid country, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var states = await _stateService.GetStates(country, limit, offset, sort, order);
            return Ok(states);
        }

        [HttpPut]
        [Route("{country:guid}/states/{state:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateState(Guid country, Guid state, [FromBody] StateDetailDto stateDetailDto)
        {
            await _stateService.UpdateState(country, state, stateDetailDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{country:guid}/states/{state:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteState(Guid country, Guid state)
        {
            await _stateService.DeleteState(country, state);
            return StatusCode(204);
        }

        [HttpPost]
        [Route("{country:guid}/states/")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateState(Guid country, [FromBody] StateDetailDto StateDetailDto)
        {
            await _stateService.CreateState(country, StateDetailDto);
            return StatusCode(201);
        }
    }
}
