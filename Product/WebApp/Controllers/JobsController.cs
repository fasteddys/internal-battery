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
using Microsoft.AspNetCore.Authorization;
using UpDiddy.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;


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
                EmployeeType = job.EmploymentType.Name,
                Summary = job.Description,
                ContactEmail = job.Subscriber.Email,
                ContactName = $"{job.Subscriber.LastName}, {job.Subscriber.FirstName}",
                ContactPhone = job.Subscriber.PhoneNumber
            };


            return View("JobDetails", jdvm);
        }

        [Authorize]
        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpGet("apply/{JobGuid}")]
        public async Task<IActionResult> ApplyAsync(Guid JobGuid)
        {
            JobPostingDto job = null;
            try
            {
                job = await _api.GetJobAsync(JobGuid);
            }
            catch (ApiException e)
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
            

            
            return View("Apply", new JobApplicationViewModel() {
                Email = this.subscriber.Email,
                FirstName = string.IsNullOrEmpty(this.subscriber.FirstName) ? string.Empty : this.subscriber.FirstName,
                LastName = string.IsNullOrEmpty(this.subscriber.LastName) ? string.Empty : this.subscriber.LastName,
                Job = job,
                JobPostingGuid = JobGuid,
                HasResumeOnFile = this.subscriber.Files.Count > 0
            });
        }

        [Authorize]
        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpPost]
        public async Task<IActionResult> SubmitApplicationAsync(JobApplicationViewModel JobApplicationViewModel)
        {
            if (this.subscriber.Files.Count == 0)
                return BadRequest();

            JobPostingDto job = null;
            try
            {
                job = await _api.GetJobAsync(JobApplicationViewModel.JobPostingGuid);
            }
            catch (ApiException e)
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

            this.subscriber.FirstName = JobApplicationViewModel.FirstName;
            this.subscriber.LastName = JobApplicationViewModel.LastName;

            JobApplicationDto jadto = new JobApplicationDto()
            {
                JobPosting = job,
                Subscriber = this.subscriber,
                CoverLetter = JobApplicationViewModel.CoverLetter
            };


            BasicResponseDto Response = null;

            CompletedJobApplicationViewModel cjavm = new CompletedJobApplicationViewModel();

            try
            {
                Response = await _api.ApplyToJobAsync(jadto);
                cjavm.JobApplicationStatus = CompletedJobApplicationViewModel.ApplicationStatus.Success;
            }
            catch(ApiException e)
            {
                cjavm.JobApplicationStatus = CompletedJobApplicationViewModel.ApplicationStatus.Failed;
                cjavm.Description = e.ResponseDto.Description;
            }


            return View("Finish", cjavm);
        }
    }
}
