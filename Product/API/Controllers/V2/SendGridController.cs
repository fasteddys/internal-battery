using System.Net;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto.User;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class SendGridController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISubscriberService _subscriberService;
        private readonly ISendGridEventService _sendGridEventService;
        private ISysEmail _sysEmail;

        public SendGridController(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
            _subscriberService = services.GetService<ISubscriberService>();
            _sendGridEventService = services.GetService<ISendGridEventService>();
            _sysEmail = services.GetService<ISysEmail>();
        }

        [HttpPost]
        [Route("LogEvent")]
        public async Task<IActionResult> LogEvent([FromBody] List<SendGridEventDto> events)       
        {
            //todo jab discuss this approach with Brent if this approach is secure enough 
            // validate the event log to be from sendgrid, if not return not found 
            if ( Request.Query["key"].ToString() == null || Request.Query["key"].ToString() != _configuration["SysEmail:EventHookApiKey"])
                return StatusCode(404);


            _sendGridEventService.AddSendGridEvents(events);
            return Ok();
        }



        [HttpGet] 
        public async Task<IActionResult> test()
        {


           await  _sysEmail.SendEmailAsync("jibrazil@populusgroup.com", "Test Email From SendGridController", "Hello World!", Constants.SendGridAccount.Transactional);


            return Ok("Hello World");
        }

    }


}
