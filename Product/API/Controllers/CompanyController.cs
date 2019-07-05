using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers
{
    public class CompanyController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ICompanyService _companyService;
        public CompanyController(ILogger<CompanyController> logger, ICompanyService companyService)
        {
            _logger = logger;
            _companyService = companyService;
        }

        [Authorize]
        [HttpGet]
        [Route("api/companies")]
        public async Task<IActionResult> CompaniesAsync()
        {
            try
            {
                var companies=await _companyService.GetCompaniesAsync();

                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"CompanyController.CompaniesAsync : Error occured when retrieving companies with message={ex.Message}", ex);
                return StatusCode(500);
            }
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpPost]
        [Route("api/company/add")]
        public async Task<IActionResult> AddCompanyAsync([FromBody]CompanyDto company)
        {
            try
            {
                await _companyService.AddCompanyAsync(company);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"CompanyController.AddCompanyAsync : Error occured when adding company with message={ex.Message}", ex);
                return StatusCode(500);
            }
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpPost]
        [Route("api/company/update")]
        public async Task<IActionResult> UpdateCompanyAsync([FromBody]CompanyDto company)
        {
            try
            {
                if (ModelState.IsValid && company != null && company.CompanyGuid != Guid.Empty)
                {
                    await _companyService.EditCompanyAsync(company);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"CompanyController.UpdateCompanyAsync : Error occured when updating company with message={ex.Message}", ex);
                return StatusCode(500);
            }

        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpDelete]
        [Route("api/company/delete/{companyGuid}")]
        public async Task<IActionResult> DeleteCompanyAsync(Guid companyGuid)
        {
            try
            {
                if (ModelState.IsValid && companyGuid != Guid.Empty)
                {
                    await _companyService.DeleteCompanyAsync(companyGuid);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"CompanyController.DeleteCompanyAsync : Error occured when deleting company with message={ex.Message}", ex);
                return StatusCode(500);
            }

        }
    }
}