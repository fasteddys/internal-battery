using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class TalentController : Controller
    {
        // GET: /<controller>/
        public IActionResult Subscribers()
        {
            List<SubscriberDto> subscribers = new List<SubscriberDto>();
            for (int i = 0; i < 10; i++)
            {
                SubscriberDto subscriber = new SubscriberDto()
                {
                    FirstName = "Person_" + i.ToString()
                };
                subscribers.Add(subscriber);
            }
            SubscribersViewModel subscribersViewModel = new SubscribersViewModel() { Subscribers = subscribers };
            return View(subscribersViewModel);
        }
    }
}
