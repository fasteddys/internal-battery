using Microsoft.AspNetCore.Mvc;

namespace CareerCircleAPI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}