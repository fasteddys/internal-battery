using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using UpDiddy.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{

    [Route("[controller]")]
    public class JobsController : BaseController
    {

        private IApi _api;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public JobsController(IApi api,
        IConfiguration configuration,
        IHostingEnvironment env)
         : base(api)
        {
            _api = api;
            _env = env;
            _configuration = configuration;
        }

        [HttpGet("browse")]
        public async Task<IActionResult> Index(string keywords, string location, int? page)
        {
            //get pageCount from Configuration file
            int pageCount=_configuration.GetValue<int>("Pagination:PageCount");

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                 jobSearchResultDto = await _api.GetJobsByLocation(
                                      keywords, location);
            }
            catch(ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }

            if (jobSearchResultDto == null)
                return NotFound();
            

            JobSearchViewModel jobSearchViewModel = new JobSearchViewModel()
            {
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(page ?? 1, pageCount)
            };

            return View("Index", jobSearchViewModel);
        }

        [HttpGet("{JobGuid}")]
        public async Task<IActionResult> JobAsync(Guid JobGuid)
        {
            JobPostingDto job = null;
            try
            {
                job = await _api.GetJobAsync(JobGuid);
            }
            catch(ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }
            
            if (job == null)
                return NotFound();

            JobDetailsViewModel jdvm = new JobDetailsViewModel
            {
                Name = job.Title,
                Company = job.Company.CompanyName,
                PostedDate = job.PostingDateUTC.ToLocalTime().ToString(),
                Location = $"{job.City}, {job.Province}, {job.Country}",
                PostingId = job.JobPostingGuid.ToString(),
                EmployeeType = job.EmploymentType?.Name,
                Summary = job.Description
            };


            return View("JobDetails", jdvm);
        }
    }
}
