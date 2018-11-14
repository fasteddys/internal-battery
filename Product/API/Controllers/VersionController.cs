using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UpDiddyApi.Controllers
{
    public class VersionController : Controller
    {

        // GET: api/values
        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {
            return Content(typeof(Startup).Assembly.GetName().Version.ToString());
        }


    }
}
