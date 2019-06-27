using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi.Controllers
{
    public class VersionController : Controller
    {
        private string _tag { get; set; }

        public VersionController(IConfiguration configuration)
        {
            _tag = configuration["Version:Tag"];
        }
        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            return Content($"Version: {typeof(Startup).Assembly.GetName().Version.ToString()}");
        }

        [HttpGet]
        [Route("[controller]/tag")]
        public IActionResult Tag()
        {
            return Content(_tag);
        }
    }
}