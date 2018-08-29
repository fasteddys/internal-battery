using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class UserController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        // GET: /<controller>/
        [Route("User/Dashboard/{UserName}")]
        public IActionResult Dashboard(string UserName)
        {
            UserViewModel VM = new UserViewModel();
            VM.UserName = UserName;

            return View(VM);
        }

    }
}
