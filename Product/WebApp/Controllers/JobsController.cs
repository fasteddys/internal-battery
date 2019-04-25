using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpDiddy.Api;
using UpDiddy.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{

    [Route("[controller]")]
    public class JobsController : BaseController
    {
        public JobsController(IApi api)
            : base(api)
        {

        }
        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var countries = await _Api.GetCountriesAsync();
            //get default states
            var states = await _Api.GetStatesByCountryAsync(null);

            JobsSearchCriteriaViewModel jobsSearchCriteriaViewModel = new JobsSearchCriteriaViewModel()
            {
                Countries = countries.Select(c => new SelectListItem()
                {
                    Text = c.DisplayName,
                    Value = c.CountryGuid.ToString()
                }),
                States=states.Select(s=>new SelectListItem()
                {
                    Text=s.Name,
                    Value=s.StateGuid.ToString()
                })

            };

            return View(jobsSearchCriteriaViewModel);
        }

        [HttpGet("{JobGuid}")]
        public IActionResult Job(Guid JobGuid)
        {
            return View();
        }

        //public IActionResult GetJobsAsync()
    }
}
