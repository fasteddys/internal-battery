using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
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
        public async Task<IActionResult> GetCountries(int limit, int take, string sort, string order)
        {
            var countries = await _countryService.GetAllCountries(limit, take, sort, order);
            return Ok(countries);
        }

        [HttpPut]
        [Route("/V2/[controller]/{country}")]
        public async Task<IActionResult> UpdateCountry(Guid country, [FromBody]  CountryDetailDto countryDetailDto)
        {
            await _countryService.UpdateCountry(country, countryDetailDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("/V2/[controller]/{country}")]
        public async Task<IActionResult> DeleteCountry(Guid country)
        {
            await _countryService.DeleteCountry(country);
            return StatusCode(204);
        }

        [HttpPost]
        [Route("/V2/[controller]/")]
        public async Task<IActionResult> CreateCountry([FromBody] CountryDetailDto countryDetailDto)
        {
            await _countryService.CreateCountry(countryDetailDto);
            return StatusCode(201);
        }
    }
}