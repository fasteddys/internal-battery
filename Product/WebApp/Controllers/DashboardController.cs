using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddy.Api;
using UpDiddy.Authentication;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    public class DashboardController : BaseController
    {

        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IApi _api;


        public DashboardController(
            IApi api,
            IConfiguration configuration,
            IHostingEnvironment env
            ) : base(api)
        {
            _env = env;
            _configuration = configuration;
            _api = api;
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            DashboardViewModel DashboardViewModel = new DashboardViewModel
            {
                Notifications = this.subscriber.Notifications
            };

            return View(DashboardViewModel);
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpPost]
        [Route("/Dashboard/SubscriberHasReadNotification")]
        public async Task<IActionResult> SubscriberHasReadNotification([FromBody] NotificationDto Notification)
        {
            BasicResponseDto response = await _api.UpdateSubscriberNotificationAsync((Guid)this.subscriber.SubscriberGuid, Notification);

            switch (response.StatusCode)
            {
                case 200:
                    return Ok();
                default:
                    return Ok();
            }


        }
    }
}
