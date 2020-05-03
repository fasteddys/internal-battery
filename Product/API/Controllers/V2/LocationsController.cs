using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.ApplicationCore.ActionFilter;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [ApiVersion("2.1")]
    [Route("/v{version:apiVersion}/[controller]/")]
    [ServiceFilter(typeof(ActionFilter))]
    public class LocationsController : BaseApiController
    {
        /* NOTE: The redundancy between url parameters and DTO properties is intentional (e.g. the update country
         * api method has a url parameter for the country guid which also exists in the DTO passed in the body). 
         * This redundancy enables our integration tests to clean up after themselves. If we make that process more
         * intelligent, we can remove this redudancy - but that will require front-end changes as well and likely
         * a new api version.
         */
        private readonly ICountryService _countryService;
        private readonly IStateService _stateService;
        private readonly ICityService _cityService;
        private readonly IPostalService _postalService;

        public LocationsController(ICountryService countryService, IStateService stateService, ICityService cityService, IPostalService postalService)
        {
            _countryService = countryService;
            _stateService = stateService;
            _cityService = cityService;
            _postalService = postalService;
        }

        #region Countries

        [HttpGet]
        [Route("countries")]
        public async Task<IActionResult> GetCountries(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var countries = await _countryService.GetAllCountries(limit, offset, sort, order);
            return Ok(countries);
        }

        [HttpPost]
        [Route("countries")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateCountry([FromBody] CountryDetailDto countryDetailDto)
        {
            var countryGuid = await _countryService.CreateCountry(countryDetailDto);
            return StatusCode(201, countryGuid);
        }

        [HttpGet]
        [Route("countries/{country:guid}")]
        public async Task<IActionResult> GetCountryDetail(Guid country)
        {
            var countryDetail = await _countryService.GetCountryDetail(country);
            return Ok(countryDetail);
        }

        [HttpPut]
        [Route("countries/{country:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCountry(Guid country, [FromBody] CountryDetailDto countryDetailDto)
        {
            countryDetailDto.CountryGuid = country;
            await _countryService.UpdateCountry(countryDetailDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("countries/{country:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteCountry(Guid country)
        {
            await _countryService.DeleteCountry(country);
            return StatusCode(204);
        }

        #endregion

        #region States

        [HttpGet]
        [Route("states/cities/lookup")]
        public async Task<IActionResult> GetCitiesAndStatesLookup([FromQuery] string value)
        {
            var citiesAndStates = await _cityService.GetCitiesAndStatesLookup(value);
            return Ok(citiesAndStates);
        }

        [HttpGet]
        [Route("countries/{country:guid}/states")]
        public async Task<IActionResult> GetStates(Guid country, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var states = await _stateService.GetStates(country, limit, offset, sort, order);
            return Ok(states);
        }

        [HttpPost]
        [Route("countries/{country:guid}/states")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateState(Guid country, [FromBody] StateDetailDto stateDetailDto)
        {
            stateDetailDto.CountryGuid = country;
            var stateGuid = await _stateService.CreateState(stateDetailDto);
            return StatusCode(201, stateGuid);
        }

        [HttpGet]
        [Route("states/{state:guid}")]
        public async Task<IActionResult> GetStateDetail(Guid state)
        {
            var stateDetail = await _stateService.GetStateDetail(state);
            return Ok(stateDetail);
        }

        [HttpPut]
        [Route("states/{state:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateState(Guid state, [FromBody] StateDetailDto stateDetailDto)
        {
            stateDetailDto.StateGuid = state;
            await _stateService.UpdateState(stateDetailDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("states/{state:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteState(Guid state)
        {
            await _stateService.DeleteState(state);
            return StatusCode(204);
        }

        #endregion

        #region Cities

        [HttpGet]
        [Route("cities/keyword")]
        public async Task<IActionResult> GetCityByKeyword([FromQuery] string value)
        {
            var cities = await _cityService.GetCityByKeyword(value);
            return Ok(cities);
        }

        [HttpGet]
        [Route("states/{state:guid}/cities")]
        public async Task<IActionResult> GetCities(Guid state, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var cities = await _cityService.GetCities(state, limit, offset, sort, order);
            return Ok(cities);
        }

        [HttpPost]
        [Route("states/{state:guid}/cities")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateCity(Guid state, [FromBody] CityDetailDto cityDetailDto)
        {
            cityDetailDto.StateGuid = state;
            var cityGuid = await _cityService.CreateCity(cityDetailDto);
            return StatusCode(201, cityGuid);
        }
        
        [HttpGet]
        [Route("cities/{city:guid}")]
        public async Task<IActionResult> GetCityDetail(Guid city)
        {
            var cityDetail = await _cityService.GetCityDetail(city);
            return Ok(cityDetail);
        }

        [HttpPut]
        [Route("cities/{city:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCity(Guid city, [FromBody] CityDetailDto cityDetailDto)
        {
            cityDetailDto.CityGuid = city;
            await _cityService.UpdateCity(cityDetailDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("cities/{city:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteCity(Guid city)
        {
            await _cityService.DeleteCity(city);
            return StatusCode(204);
        }

        #endregion

        #region Postals

        [HttpGet]
        [Route("postals/lookup")]
        public async Task<IActionResult> GetPostalsLookup([FromQuery] string value)
        {
            var citiesAndStates = await _postalService.GetPostalsLookup(value);
            return Ok(citiesAndStates);
        }

        [HttpGet]
        [Route("cities/{city:guid}/postals")]
        public async Task<IActionResult> GetPostals(Guid city, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var postals = await _postalService.GetPostals(city, limit, offset, sort, order);
            return Ok(postals);
        }

        [HttpPost]
        [Route("cities/{city:guid}/postals")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreatePostal(Guid city, [FromBody] PostalDetailDto postalDetailDto)
        {
            postalDetailDto.CityGuid = city;
            var postalGuid = await _postalService.CreatePostal(postalDetailDto);
            return StatusCode(201, postalGuid);
        }

        [HttpGet]
        [Route("postals/{postal:guid}")]
        public async Task<IActionResult> GetPostalDetail(Guid postal)
        {
            var postalDetail = await _postalService.GetPostalDetail(postal);
            return Ok(postalDetail);
        }

        [HttpPut]
        [Route("postals/{postal:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdatePostal(Guid postal, [FromBody] PostalDetailDto postalDetailDto)
        {
            postalDetailDto.PostalGuid = postal;
            await _postalService.UpdatePostal(postalDetailDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("postals/{postal:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeletePostal(Guid postal)
        {
            await _postalService.DeletePostal(postal);
            return StatusCode(204);
        }

        #endregion
    }
}