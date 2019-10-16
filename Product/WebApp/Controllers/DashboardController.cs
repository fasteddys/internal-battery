using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using UpDiddy.Api;
using UpDiddy.Authentication;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using Wangkanai.Detection;

namespace UpDiddy.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {

        private readonly IHostingEnvironment _env;
        private readonly IDeviceResolver _deviceResolver;


        public DashboardController(
            IApi api,
            IConfiguration configuration,
            IHostingEnvironment env,
            IDeviceResolver deviceResolver
            ) : base(api,configuration)
        {
            _env = env;
            _configuration = configuration;
            _deviceResolver = deviceResolver;
        }

        [LoadSubscriber(isHardRefresh: true, isSubscriberRequired: true)]
        [HttpGet]
        public IActionResult Index()
        {
            string DeviceType = _deviceResolver.Device.Type.ToString();
            this.subscriber.Notifications.Reverse();
            DashboardViewModel DashboardViewModel = new DashboardViewModel
            {
                NotificationEmailsEnabled = this.subscriber.NotificationEmailsEnabled,
                Notifications = this.subscriber.Notifications,
                DeviceType = DeviceType
            };

            return View(DashboardViewModel);
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpGet]
        [Route("/Dashboard/ToggleSubscriberNotificationEmail/{isEnabled}")]
        public async Task<bool> ToggleSubscriberNotificationEmail(string isEnabled)
        {
            bool _enabled = false;
            if (!bool.TryParse(isEnabled, out _enabled))
            {
                return false;
            }
            else
            {
                var result = await _Api.ToggleSubscriberNotificationEmailAsync((Guid)this.subscriber.SubscriberGuid, _enabled);
                return result.StatusCode == 200;
            }
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpPost]
        [Route("/Dashboard/SubscriberHasReadNotification")]
        public async Task<int> SubscriberHasReadNotification([FromBody] NotificationDto Notification)
        {
            await _Api.UpdateSubscriberNotificationAsync((Guid)this.subscriber.SubscriberGuid, Notification);
            this.subscriber = await _Api.SubscriberAsync((Guid)this.subscriber.SubscriberGuid, true);
            int newNotificationCount = 0;
            foreach (NotificationDto notification in this.subscriber.Notifications)
            {
                if (notification.HasRead == 0)
                    newNotificationCount++;
            }
            return newNotificationCount;
        }

        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpPost]
        [Route("/Dashboard/DeleteSubscriberNotification")]
        public async Task<int> DeleteSubscriberNotification([FromBody] NotificationDto Notification)
        {
            await _Api.DeleteSubscriberNotificationAsync((Guid)this.subscriber.SubscriberGuid, Notification);
            this.subscriber = await _Api.SubscriberAsync((Guid)this.subscriber.SubscriberGuid, true);
            int newNotificationCount = 0;
            foreach (NotificationDto notification in this.subscriber.Notifications)
            {
                if (notification.HasRead == 0)
                    newNotificationCount++;
            }
            return newNotificationCount;
        }


    }
}
