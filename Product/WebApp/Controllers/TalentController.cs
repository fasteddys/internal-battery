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
        public ViewResult Subscribers()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult> SubscriberGrid(String searchQuery)
        {
            IList<SubscriberDto> subscribers = await _api.SubscriberSearchAsync(searchQuery);
            return PartialView("_SubscriberGrid", subscribers);
        }

        [Authorize]
        [HttpGet]
        [Route("/Talent/Subscriber/{subscriberGuid}")]
        public async Task<IActionResult> SubscriberAsync(Guid subscriberGuid)
        {
            SubscriberDto subscriber = await _api.SubscriberAsync(subscriberGuid, false);

            SubscriberViewModel subscriberViewModel = new SubscriberViewModel()
            {
                FirstName = subscriber.FirstName,
                LastName = subscriber.LastName,
                Email = subscriber.Email,
                PhoneNumber = subscriber.PhoneNumber,
                Address = subscriber.Address,
                City = subscriber.City,
                State = subscriber.State?.Code,
                Country = subscriber.State?.Country?.Code3,
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

            return View("Subscriber", subscriberViewModel);
        }
    }
}
