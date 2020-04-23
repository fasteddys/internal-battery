using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models.HubSpot;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]

    public class HubSpotController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;  
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IHangfireService _hangfireService;
        private readonly IConfiguration _configuration;
        private readonly IHubSpotService _hubSpotService;


        public HubSpotController(UpDiddyDbContext db, ILogger<HubSpotController> sysLog, FileContentResult pixelResponse, IRepositoryWrapper repositoryWrapper, IHangfireService hangfireService, IServiceProvider services)
        {            
            _db = db;
            _syslog = sysLog;            
            _repositoryWrapper = repositoryWrapper;
            _hangfireService = hangfireService;
            _hubSpotService = services.GetService<IHubSpotService>();
            _configuration = services.GetService<IConfiguration>();
        }


        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("Test")]
        public async Task<IActionResult> test([FromBody] HubSpotContactDto contactDto)
        {

           long vid = await  _hubSpotService.AddOrUpdateContactBySubscriberGuid(Guid.Parse("71A7156E-173F-4054-83ED-AD6127BAFE87"),false);


            return Ok();
        }



    }
}
