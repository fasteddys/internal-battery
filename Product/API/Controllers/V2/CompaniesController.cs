using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/companies/")]
    public class CompanieController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanieController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var companies = await _companyService.GetCompanies(limit, offset, sort, order);
            return Ok(companies);
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _companyService.GetCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet]
        [Route("{company:guid}")]
        public async Task<IActionResult> GetCompany(Guid company)
        {
            var result = await _companyService.GetByCompanyGuid(company);
            return Ok(result);
        }

        [HttpPut]
        [Route("{company:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCompany(Guid company, [FromBody]  CompanyDto companyDto)
        {
            companyDto.CompanyGuid = company;
            await _companyService.EditCompanyAsync(companyDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{company:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteCompany(Guid company)
        {
            await _companyService.DeleteCompanyAsync(company);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyDto companyDto)
        {
            await _companyService.AddCompanyAsync(companyDto);
            return StatusCode(201);
        }
    }
}
