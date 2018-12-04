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
