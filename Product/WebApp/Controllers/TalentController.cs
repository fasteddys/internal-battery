using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using UpDiddy.Api;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    [Authorize(Policy = "IsRecruiterPolicy")]
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
            var subscriberSourcesDto = _api.SubscriberSourcesAsync().Result.OrderByDescending(ss => ss.Count);

            var selectListItems = subscriberSourcesDto.Select(ss => new SelectListItem()
            {
                Text = $"{ss.Referrer} ({ss.Count})",
                Value = ss.Referrer
            })
            .AsEnumerable();

            return View(new TalentSubscriberViewModel() { SubscriberSources = selectListItems });
        }

        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult> SubscriberGrid(string searchAndFilter)
        {
            string searchFilter;
            string searchQuery;

            if (searchAndFilter != null)
            {
                var jObject = JObject.Parse(searchAndFilter);
                searchFilter = jObject["searchFilter"].Value<string>();
                searchQuery = jObject["searchQuery"].Value<string>();
            }
            else
            {
                searchFilter = "any";
                searchQuery = string.Empty;
            }
            IList<SubscriberDto> subscribers = await _api.SubscriberSearchAsync(searchFilter, searchQuery);
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

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpDelete]
        [Route("/Talent/Subscriber/{subscriberGuid}")]
        public async Task<IActionResult> DeleteSubscriberAsync(Guid subscriberGuid)
        {
            var isSubscriberDeleted = await _api.DeleteSubscriberAsync(subscriberGuid);
            return new JsonResult(isSubscriberDeleted);
        }
    }
}
