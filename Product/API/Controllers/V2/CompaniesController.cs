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
        public async Task<IActionResult> GetCompanies(int limit, int offset, string sort, string order)
        {
            var companys = await _companyService.GetCompanies(limit, offset, sort, order);
            return Ok(companys);
        }

        [HttpGet]
        [Route("{company}")]
        public async Task<IActionResult> GetCompany(Guid company)
        {
            var result = await _companyService.GetByCompanyGuid(company);
            return Ok(result);
        }

        [HttpPut]
        [Route("{company}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateCompany(Guid company, [FromBody]  CompanyDto companyDto)
        {
            companyDto.CompanyGuid = company;
            await _companyService.EditCompanyAsync(companyDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{company}")]
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
