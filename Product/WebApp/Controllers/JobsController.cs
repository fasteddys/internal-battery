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
using X.PagedList;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{

    
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
        
        [HttpGet("[controller]")]
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

        
        [HttpGet]
        [Route("[controller]/{JobGuid}")]
        [Route("[controller]/{industry}/{category}/{country}/{state}/{city}/{JobGuid}")]
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
                    case (404):
                        job = await _api.GetExpiredJobAsync(JobGuid);
                        if (job != null)
                        {
                            string location = job?.City + ", " + job?.Province;
                            JobSearchResultDto jobSearchResultDto = await _api.GetJobsByLocation(job.Title, location);
                            int pageCount = _configuration.GetValue<int>("Pagination:PageCount");

                            if (jobSearchResultDto == null)
                                return NotFound();

                            var jobSearchViewModel = new JobSearchViewModel()
                            {
                                Keywords = job.Title,
                                Location = location,
                                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount)
                            };

                            // Remove the expired job link from the search provider's index.
                            Response.StatusCode = 404;
                            return View("Index", jobSearchViewModel);
                        }
                        break;
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }
            
            if (job == null)
                return NotFound();

            // check to see if the inbound url matches the semantic url
            if (job.SemanticJobPath.ToLower() != Request.Path.Value.ToLower())
            {
                // if it does not match, redirect to the semantic url
                return RedirectPermanent(job.SemanticJobPath.ToLower());
            }

            JobDetailsViewModel jdvm = new JobDetailsViewModel
            {
                Name = job.Title,
                Company = job.Company?.CompanyName,
                PostedDate = job.PostingDateUTC == null ? string.Empty : job.PostingDateUTC.ToLocalTime().ToString(),
                Location = $"{job.City}, {job.Province}, {job.Country}",
                PostingId = job.JobPostingGuid?.ToString(),
                EmployeeType = job.EmploymentType?.Name,
                Summary = job.Description,

            };

            // Display subscriber info if it exists
            if ( job.Recruiter.Subscriber != null )
            {
                jdvm.ContactEmail = job.Recruiter.Subscriber?.Email;
                jdvm.ContactName = $"{job.Recruiter.Subscriber?.LastName}, {job.Recruiter.Subscriber?.FirstName}";
                jdvm.ContactPhone = job.Recruiter.Subscriber?.PhoneNumber;

            }
            else // Use recruiter info in no subscriber exists
            {
                jdvm.ContactEmail = job.Recruiter?.Email;
                jdvm.ContactName = $"{job.Recruiter?.LastName}, {job.Recruiter?.FirstName}";
                jdvm.ContactPhone = job.Recruiter?.PhoneNumber;

            }
 


            return View("JobDetails", jdvm);
        }

        [Authorize]
        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpGet("[controller]/apply/{JobGuid}")]
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
        [HttpPost("[controller]")]
        public async Task<IActionResult> SubmitApplicationAsync(JobApplicationViewModel JobApplicationViewModel)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(500);
            }

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
