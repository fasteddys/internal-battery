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
using Wangkanai.Detection;

namespace UpDiddy.Controllers
{
    public class DashboardController : BaseController
    {

        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IApi _api;
        private readonly IDeviceResolver _deviceResolver;


        public DashboardController(
            IApi api,
            IConfiguration configuration,
            IHostingEnvironment env,
            IDeviceResolver deviceResolver
            ) : base(api)
        {
            _env = env;
            _configuration = configuration;
            _api = api;
            _deviceResolver = deviceResolver;
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string DeviceType = _deviceResolver.Device.Type.ToString();
            DashboardViewModel DashboardViewModel = new DashboardViewModel
            {
                Notifications = this.subscriber.Notifications,
                DeviceType = DeviceType
            };

            return View(DashboardViewModel);
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpPost]
        [Route("/Dashboard/SubscriberHasReadNotification")]
        public async Task<int> SubscriberHasReadNotification([FromBody] NotificationDto Notification)
        {
            await _api.UpdateSubscriberNotificationAsync((Guid)this.subscriber.SubscriberGuid, Notification);
            this.subscriber = await _api.SubscriberAsync((Guid)this.subscriber.SubscriberGuid, true);
            int newNotificationCount = 0;
            foreach(NotificationDto notification in this.subscriber.Notifications)
            {
                if (notification.HasRead == 0)
                    newNotificationCount++;
            }
            return newNotificationCount;


        }
    }
}
