using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

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
    }
}