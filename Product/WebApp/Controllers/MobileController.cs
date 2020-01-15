using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;

namespace UpDiddy.Controllers
{
    public class MobileController : BaseController
    {

        public MobileController(IApi api, IConfiguration configuration) : base(api,configuration)
        {
        }



        /*
        public IActionResult Index()
        {
            return RedirectPermanent("https://www.careercircle.com/");
        }
        */
    }
}
