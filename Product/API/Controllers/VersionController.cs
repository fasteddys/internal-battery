using Microsoft.AspNetCore.Mvc;

namespace UpDiddyApi.Controllers
{
    public class VersionController : Controller
    {
        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            return Content($"Version: {typeof(Startup).Assembly.GetName().Version.ToString()}");
        }
    }
}