using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;

namespace UpDiddy.Controllers
{
    public class MobileController : BaseController
    {
        public MobileController(IApi api) : base(api)
        {
        }

        public IActionResult Index()
        {
            return RedirectPermanent("https://www.careercircle.com/");
        }
    }
}
