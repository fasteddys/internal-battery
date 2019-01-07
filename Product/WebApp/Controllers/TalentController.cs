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

        // GET: /<controller>/
        public IActionResult Subscribers()
        {
            IList<SubscriberDto> subscribers = _api.Subscribers();

            SubscribersViewModel subscribersViewModel = new SubscribersViewModel() { Subscribers = subscribers };
            return View(subscribersViewModel);
        }
    }
}
