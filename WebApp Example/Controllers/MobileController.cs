using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class MobileController : Controller
    {
        public IActionResult Index()
        {
            return RedirectPermanent("https://www.careercircle.com/Home/Index");
            //return View();
        }
    }
}