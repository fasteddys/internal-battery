using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    public class TalentController : Controller
    {
        private IApi _api;
        public TalentController(IApi api)
        {
            _api = api;
        }

        [Authorize]
        [HttpGet]
        public ViewResult Subscribers()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public PartialViewResult SubscriberGrid(String searchQuery)
        {
            IList<SubscriberDto> subscribers = _api.SubscriberSearch(searchQuery);
            return PartialView("_SubscriberGrid", subscribers);
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
