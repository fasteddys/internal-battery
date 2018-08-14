using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wireframe1.Controllers
{
    public class AboutController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [Route ("/About/Privacy-Policy")]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        [Route("/About/Terms-of-Service")]
        public IActionResult TermsofService()
        {
            return View();
        }

        [Route("/About/Cookie-Notice")]
        public IActionResult CookieNotice()
        {
            return View();
        }

        [Route("/About/Mandatory-Notice")]
        public IActionResult MandatoryNotice()
        {
            return View();
        }
    }
}