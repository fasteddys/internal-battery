using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Shared;

namespace UpDiddyApi.Controllers
{
    public class HangfireController : Controller
    {
        IConfiguration _configuration;

        public HangfireController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("[controller]")]
        public IActionResult Unlock()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitToken(string unlockToken)
        {
            HttpContext.Response.Cookies.Append("HangfireUnlocked", Crypto.Encrypt(_configuration["Crypto:Key"], unlockToken));
            return Redirect("/dashboard");
        }
    }
}
