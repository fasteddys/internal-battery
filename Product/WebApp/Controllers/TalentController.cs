using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    [Authorize(Policy= "IsRecruiterPolicy")]
    public class TalentController : Controller
    {
        private IApi _api;
        public TalentController(IApi api)
        {
            _api = api;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Subscribers()
        {
            IList<SubscriberDto> subscribers = _api.Subscribers();
            SubscribersViewModel subscribersViewModel = new SubscribersViewModel() { Subscribers = subscribers };
            return View(subscribersViewModel);
        }


        [Authorize]
        [HttpGet]
        [Route("/Talent/Subscriber/{subscriberGuid}")]
        public IActionResult Subscriber(Guid subscriberGuid)
        {
            SubscriberDto subscriber = _api.Subscriber(subscriberGuid);

            SubscriberViewModel subscriberViewModel = new SubscriberViewModel()
            {
                FirstName = subscriber.FirstName,
                LastName = subscriber.LastName,
                Email = subscriber.Email,
                PhoneNumber = subscriber.PhoneNumber,
                Address = subscriber.Address,
                City = subscriber.City,
                State = subscriber.State?.Name,
                Country = subscriber.State?.Country?.DisplayName,
                FacebookUrl = subscriber.FacebookUrl,
                GithubUrl = subscriber.GithubUrl,
                LinkedInUrl = subscriber.LinkedInUrl,
                StackOverflowUrl = subscriber.StackOverflowUrl,
                TwitterUrl = subscriber.TwitterUrl,
                WorkHistory = subscriber.WorkHistory,
                EducationHistory = subscriber.EducationHistory,
                Skills = subscriber.Skills,
                Enrollments = subscriber.Enrollments
            };

            return View(subscriberViewModel);
        }
    }
}
