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
using System.Text;


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
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
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
                job = await _api.GetJobAsync(JobGuid, GoogleCloudEventsTrackingDto.Build(HttpContext.Request.Query, UpDiddyLib.Shared.GoogleJobs.ClientEventType.View));
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
                RequestId = job.RequestId,
                ClientEventId = job.ClientEventId,
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


            var trackingDto = await _api.RecordClientEventAsync(JobGuid, GoogleCloudEventsTrackingDto.Build(HttpContext.Request.Query, UpDiddyLib.Shared.GoogleJobs.ClientEventType.Application_Start));
            return View("Apply", new JobApplicationViewModel() {
                RequestId = trackingDto?.RequestId,
                ClientEventId = trackingDto?.ClientEventId,
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

                await _api.RecordClientEventAsync(JobApplicationViewModel.JobPostingGuid, new GoogleCloudEventsTrackingDto(){
                    RequestId = JobApplicationViewModel.RequestId,
                    ParentClientEventId = JobApplicationViewModel.ClientEventId,
                    Type = UpDiddyLib.Shared.GoogleJobs.ClientEventType.Application_Finish
                });

                cjavm.JobApplicationStatus = CompletedJobApplicationViewModel.ApplicationStatus.Success;
            }
            catch(ApiException e)
            {
                cjavm.JobApplicationStatus = CompletedJobApplicationViewModel.ApplicationStatus.Failed;
                cjavm.Description = e.ResponseDto.Description;
            }


            return View("Finish", cjavm);
        }
   
        [HttpGet("Browse-Jobs")]
        public async Task<IActionResult> BrowseAsync()
        {
            BrowseJobsViewModel bjvm = new BrowseJobsViewModel
            {
                JobCategories = await _api.GetJobCategories(),
                States = await _api.GetAllStatesAsync(),
                Industries = await _api.GetIndustryAsync()
            };

            return View("Browse", bjvm);
        }

        [HttpGet("browse-jobs-industry/{industry}/{category}/{country}/{state}/{city}")]
        public async Task<IActionResult> BrowseJobsByIndustryAsync()
        {
            BrowseJobsViewModel bjvm = new BrowseJobsViewModel
            {
                JobCategories = await _api.GetJobCategories(),
                States = await _api.GetAllStatesAsync(),
                Industries = await _api.GetIndustryAsync()
            };

            return View("Browse", bjvm);
        }


        [HttpGet("browse-jobs-location/{country?}/{state?}/{city?}/{industry?}/{category?}/{page?}")]
        public async Task<IActionResult> BrowseJobsByLocationAsync(
            string country,
            string state,
            string city,
            string industry,
            string category,
            int? page)
        {
            // If user types in root url, default to US.
            if (string.IsNullOrEmpty(country))
                return Redirect($"{Request.Path}/us");


            

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _api.GetJobsByLocationUsingRoute(country, state, city, industry?.Replace("-", "+"), category?.Replace("-", "+"));
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

            if (jobSearchResultDto == null)
                return NotFound();

            int pageCount = _configuration.GetValue<int>("Pagination:PageCount");

            BrowseJobsLocationViewModel jobSearchViewModel = new BrowseJobsLocationViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(page ?? 1, pageCount)
            };

            // User has reached the end of the browse flow, so present results.
            if (!string.IsNullOrEmpty(category))
                return View("BrowseByType", jobSearchViewModel);


            JobQueryFacetDto jqfdto = FindNeededFacet("admin_1", jobSearchResultDto.Facets);

            // Return state view if user has only specified country
            if (string.IsNullOrEmpty(state))
            {
                List<DisplayItem> StateLocations = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto JobQueryFacet in jqfdto.Facets)
                {
                    UpDiddyLib.Helpers.Utils.State State;
                    Enum.TryParse(JobQueryFacet.Label.ToUpper(), out State);
                    string StateName = UpDiddyLib.Helpers.Utils.GetState(State);
                    StateLocations.Add(new DisplayItem
                    {
                        Label = $"{UpDiddyLib.Helpers.Utils.ToTitleCase(StateName)} ({JobQueryFacet.Count})",
                        Url = $"{Request.Path}/{StateName.ToLower().Replace(" ", "-")}"
                    });
                }

                BrowseJobsLocationViewModel bjlvmState = new BrowseJobsLocationViewModel()
                {
                    Items = StateLocations
                };

                return View("BrowseByType", bjlvmState);
            }

            // If flow reaches this point, user has specified country and state, but 
            // needs to choose city
            if (string.IsNullOrEmpty(city))
            {
                jqfdto = FindNeededFacet("city", jobSearchResultDto.Facets);

                // City histogram wasn't found
                if (jqfdto == null)
                    return NotFound();

                List<DisplayItem> LocationsCities = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    LocationsCities.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label} ({FacetItem.Count})",
                        Url = $"{Request.Path}/{FacetItem.Label.Split(",")[0].ToLower().Replace(" ", "-")}"
                    });
                }

                BrowseJobsLocationViewModel bjlvm = new BrowseJobsLocationViewModel()
                {
                    Items = LocationsCities
                };

                return View("BrowseByType", bjlvm);
            }

            if (string.IsNullOrEmpty(industry))
            {
                jqfdto = FindNeededFacet("industry", jobSearchResultDto.Facets);

                // Industry histogram wasn't found
                if (jqfdto == null)
                    return View("BrowseByType", jobSearchViewModel);

                List<DisplayItem> Industries = new List<DisplayItem>();
                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Industries.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label} ({FacetItem.Count})",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}"
                    });
                }

                BrowseJobsLocationViewModel bjlvm = new BrowseJobsLocationViewModel()
                {
                    Items = Industries
                };

                return View("BrowseByType", bjlvm);
            }

            if (string.IsNullOrEmpty(category))
            {
                jqfdto = FindNeededFacet("jobcategory", jobSearchResultDto.Facets);

                // Job category histogram wasn't found
                if (jqfdto == null)
                    return View("BrowseByType", jobSearchViewModel);

                List<DisplayItem> Categories = new List<DisplayItem>();
                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Categories.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label} ({FacetItem.Count})",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}/1"
                    });
                }

                BrowseJobsLocationViewModel bjlvm = new BrowseJobsLocationViewModel()
                {
                    Items = Categories
                };

                return View("BrowseByType", bjlvm);
            }

            return NotFound();

            
            
        }

        #region Browse job by location helpers

        private JobQueryFacetDto FindNeededFacet(string key, List<JobQueryFacetDto> List)
        {
            foreach(JobQueryFacetDto facet in List)
            {
                if (facet.Name.ToLower().Equals(key.ToLower()))
                    return facet;
            }

            return null;
        }

        private string FormatKeywords(string industry, string category)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((!string.IsNullOrEmpty(industry) || !string.IsNullOrEmpty(category)) ? "?" : string.Empty);
            sb.Append(industry == null ? string.Empty : "industry=" + industry.Replace("-", "+"));
            sb.Append((!string.IsNullOrEmpty(industry) && !string.IsNullOrEmpty(category)) ? "&" : string.Empty);
            sb.Append(category == null ? string.Empty : "jobcategory=" + category.Replace("-", "+"));
            return sb.ToString();
        }

        private string FormatLocation(string country, string state, string city)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(country ?? string.Empty);
            sb.Append((!string.IsNullOrEmpty(country) && (!string.IsNullOrEmpty(state) || !string.IsNullOrEmpty(city))) ? "," : string.Empty);
            sb.Append(state ?? string.Empty);
            sb.Append((!string.IsNullOrEmpty(state) && !string.IsNullOrEmpty(city)) ? "," : string.Empty);
            sb.Append(city ?? string.Empty);
            return sb.ToString();
        }

        #endregion

    }
}
