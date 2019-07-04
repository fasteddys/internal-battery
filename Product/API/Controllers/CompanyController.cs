using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers
{
    
    //[ApiController]
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
                
            }
            return BadRequest();
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpPost]
        [Route("api/company/add")]
        public async Task<IActionResult> AddCompanyAsync([FromBody]CompanyDto company)
        {
            await _companyService.AddCompanyAsync(company);
            return Ok();
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpPost]
        [Route("api/company/edit")]
        public async Task<IActionResult> EditCompanyAsync([FromBody]CompanyDto company)
        {
            if(ModelState.IsValid && company != null && company.CompanyGuid!=Guid.Empty)
            {
                await _companyService.EditCompanyAsync(company);
                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpDelete]
        [Route("api/company/delete/{companyGuid}")]
        public async Task<IActionResult> DeleteCompanyAsync(Guid companyGuid)
        {
            //Guid guid=Guid.Parse(companyGuid);
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
    }
}