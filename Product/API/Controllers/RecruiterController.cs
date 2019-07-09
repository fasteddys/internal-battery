using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers
{
    public class RecruiterController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IRecruiterService _recruiterService;
        public RecruiterController(ILogger<RecruiterController> logger, IRecruiterService recruiterService)
        {
            _logger = logger;
            _recruiterService = recruiterService;
        }

        [Authorize]
        [HttpGet]
        [Route("api/recruiters")]
        public async Task<IActionResult> RecruitersAsync()
        {
            try
            {
                var recruiters = await _recruiterService.GetRecruitersAsync();

                return Ok(recruiters);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"RecruiterController.RecruitersAsync : Error occured when retrieving recruiters with message={ex.Message}", ex);
                return StatusCode(500);
            }
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpPost]
        [Route("api/[controller]/add")]
        public async Task<IActionResult> AddRecruiterAsync([FromBody]RecruiterDto company)
        {
            try
            {
                await _recruiterService.AddRecruiterAsync(company);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"RecruiterController.AddRecruiterAsync : Error occured when adding recruiter with message={ex.Message}", ex);
                return StatusCode(500);
            }
        }
    }
}