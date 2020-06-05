using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("new-users")]
        public async Task<IActionResult> GetNewUsers()
        {
            var users = await _reportsService.GetNewUsers();
            return Ok(users);
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("all-users-detail")]
        public async Task<IActionResult> GetAllUsersDetail()
        {
            var userDetails = await _reportsService.GetAllUsersDetail();
            return Ok(userDetails);
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("all-hiring-managers-detail")]
        public async Task<IActionResult> GetAllHiringManagersDetail()
        {
            var hiringManagerDetails = await _reportsService.GetAllHiringManagersDetail();
            return Ok(hiringManagerDetails);
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("users-by-partner/{partner:guid}")]
        public async Task<IActionResult> GetUsersByPartnerDetail(Guid partner, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var users = await _reportsService.GetUsersByPartnerDetail(partner, startDate, endDate);
            return Ok(users);
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("users-by-partner")]
        public async Task<IActionResult> GetUsersByPartner([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var users = await _reportsService.GetUsersByPartner(startDate, endDate);
            return Ok(users);
        }
    }
}