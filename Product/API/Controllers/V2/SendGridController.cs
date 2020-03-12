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
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class SendGridController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISubscriberService _subscriberService;
        private readonly ISendGridEventService _sendGridEventService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISubscriberEmailService _subscriberEmailService;
        private ISysEmail _sysEmail;
        private readonly ILogger _syslog;

        public SendGridController(IServiceProvider services, ILogger<SendGridController> syslog)
        {
            _configuration = services.GetService<IConfiguration>();
            _subscriberService = services.GetService<ISubscriberService>();
            _sendGridEventService = services.GetService<ISendGridEventService>();
            _sysEmail = services.GetService<ISysEmail>();
            _repositoryWrapper = services.GetService<IRepositoryWrapper>();
            _subscriberEmailService = services.GetService<ISubscriberEmailService>();
            _syslog = syslog;
        }

        [HttpPost]
        [Route("LogEvent")]
        public async Task<IActionResult> LogEvent([FromBody] List<SendGridEventDto> events)
        {
            // Use secret key to authorize sendgrid event logging.  The documentation found here https://sendgrid.com/docs/for-developers/tracking-events/event/ 
            // does not adquately document how to secure the implemented webhook authorization.  Stackoverflow suggests two approaches here
            // https://stackoverflow.com/questions/20865673/sendgrid-incoming-mail-webhook-how-do-i-secure-my-endpoint
            if (Request.Query["key"].ToString() == null || Request.Query["key"].ToString() != _configuration["SysEmail:EventHookApiKey"])
                return StatusCode(404);

            _sendGridEventService.AddSendGridEvents(events);
            return Ok();
        }


        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("PurgeAuditRecords")]
        public async Task<IActionResult> PurgeAuditRecords(int lookbackDays)
        {

            if (lookbackDays == 0)
                lookbackDays = int.Parse(_configuration["CareerCircle:SendGridAuditPurgeLookBackDays"]);


            var rval = _sendGridEventService.PurgeSendGridEvents(lookbackDays);
            return Ok(rval);
        }

        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("Statistics/{SubscriberGuid}")]
        public async Task<IActionResult> GetSUbscriberStatistics(Guid subscriberGuid)
        {
            var rval = await _subscriberEmailService.GetEmailStatistics(subscriberGuid);
            return Ok(rval);
        }


        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("Test")]
        public async Task<JsonResult> test()
        {
            string debugEmail = _configuration[$"SysEmail:SystemDebugEmailAddress"];

            var notifySystemResult = await _sysEmail.SendEmailAsync(debugEmail, "Test for NotifySystem SendGrid Account", "test", Constants.SendGridAccount.NotifySystem);
            var transactionalResult = await _sysEmail.SendEmailAsync(debugEmail, "Test for Transactional SendGrid Account", "test", Constants.SendGridAccount.Transactional);

            return Json(new { NotifySystem = notifySystemResult, Transactional = transactionalResult });
        }
    }
}
