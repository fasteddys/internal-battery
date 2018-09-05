using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class CourseController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
 
        // GET: /<controller>/
        [Route("Course/ByTopic/{Topic}")]
        public IActionResult ByTopic(string Topic)
        {
            CourseViewModel VM = new CourseViewModel
            {
                CourseName = Topic
            };
            return View(VM);
        }

        // GET: /<controller>/
        [Route("Course/Details/{Course}")]
        public IActionResult Details(string Course)
        {
            CourseViewModel VM = new CourseViewModel
            {
                CourseName = Course
            };
            return View(VM);
        }

        // GET: /<controller>/
        [Route("Course/Checkout/{Course}")]
        public IActionResult Checkout(string Course)
        {
            CourseViewModel VM = new CourseViewModel
            {
                CourseName = Course
            };
            return View(VM);
        }



    }
}
