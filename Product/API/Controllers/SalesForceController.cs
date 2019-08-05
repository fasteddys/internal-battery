
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class SalesForceController : ControllerBase
    {
        private readonly ILogger _syslog;

        private readonly ISalesForceService _salesForceService;

        public SalesForceController(ISalesForceService salesForceService, ILogger<SalesForceController> sysLog)

        {
            _salesForceService = salesForceService;
            _syslog = sysLog;
        }

        // GET: api/topics/id
       [HttpPut("api/[controller]/sign-up")]
        public async Task<IActionResult> SignUpForWaitList(SalesForceWaitListDto dto)
        {
            try
            {
                await _salesForceService.AddToWaitList(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Error, $"CompanyController.AddCompanyAsync : Error occured when adding company with message={ex.Message}", ex);
                return StatusCode(500);
            }
        }

    }
}