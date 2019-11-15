using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.Controllers.V2
{
    [ApiController]
    public class SubscribersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISubscriberService _subscriberService;

        public SubscribersController(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
            _subscriberService = services.GetService<ISubscriberService>();
        }


        [HttpPost]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        [Route("/V2/[controller]/new-subscriber-registration")]
        public async Task<IActionResult> NewSubscriberRegistration([FromBody] SubscriberDto subscriberDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var existingSubscriber = await _subscriberService.GetSubscriberByEmail(subscriberDto.Email);
            if (existingSubscriber != null)
                return Conflict();

            var newSubscriberGuid = await _subscriberService.CreateSubscriberAsync(subscriberDto);
            return Ok(new { subscriberGuid = newSubscriberGuid });
        }

    }
}