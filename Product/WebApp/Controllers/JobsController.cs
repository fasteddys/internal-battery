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
using System.Text.RegularExpressions;


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


        [HttpGet("browse-jobs-location/{country?}")]
        [HttpGet("browse-jobs-location/{country}/{state?}/{page:int?}")]
        [HttpGet("browse-jobs-location/{country}/{state}/{city?}/{page:int?}")]
        [HttpGet("browse-jobs-location/{country}/{state}/{city}/{industry?}/{page:int?}")]
        [HttpGet("browse-jobs-location/{country}/{state}/{city}/{industry}/{category?}/{page:int?}")]
        public async Task<IActionResult> BrowseJobsByLocationAsync(
            string country,
            string state,
            string city,
            string industry,
            string category,
            int page)
        {
            // If user types in root url, default to US.
            if (string.IsNullOrEmpty(country))
                return RedirectPermanent($"{Request.Path}/us");


            if (!string.IsNullOrEmpty(country) &&
                !string.IsNullOrEmpty(state) &&
                !string.IsNullOrEmpty(city) &&
                !string.IsNullOrEmpty(industry) &&
                !string.IsNullOrEmpty(category) &&
                page == 0)
                return RedirectPermanent($"{Request.Path}/1");

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _api.GetJobsByLocationUsingRoute(
                    country, 
                    state, 
                    city, 
                    industry?.Replace("-", "+"), 
                    category?.Replace("-", "+"), 
                    page);
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

            if (page > jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0) || page < 0)
                return NotFound();

            if (jobSearchResultDto == null || !IsValidUrl(jobSearchResultDto, country, state, city, industry, category))
                return NotFound();

            int pageCount = _configuration.GetValue<int>("Pagination:PageCount");

            BrowseJobsLocationViewModel jobSearchViewModel = new BrowseJobsLocationViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount),
                CurrentPage = page,
                NumberOfPages = jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0)
            };

            DeterminePaginationRange(ref jobSearchViewModel);

            // User has reached the end of the browse flow, so present results.
            if (!string.IsNullOrEmpty(category) || page != 0)
            {
                jobSearchViewModel.BaseUrl = AssembleBaseUrl(country, state, city, industry, category);
                return View("BrowseByType", jobSearchViewModel);
            }
                


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
                        Url = $"{Request.Path}/{JobQueryFacet.Label.ToLower()}"
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
                    return RedirectPermanent(Request.Path + "/1");

                List<DisplayItem> LocationsCities = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Regex rgx = new Regex("[^a-zA-Z]");
                    LocationsCities.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label} ({FacetItem.Count})",
                        Url = $"{Request.Path}/{rgx.Replace(FacetItem.Label.Split(",")[0].ToLower(), "-")}"
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

                List<DisplayItem> Industries = new List<DisplayItem>();


                if (jqfdto?.Facets == null)
                    return RedirectPermanent(Request.Path + "/1");


                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Industries.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label} ({FacetItem.Count})",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}"
                    });
                }
                return View("BrowseByType", new BrowseJobsLocationViewModel() { Items = Industries });
            }

            if (string.IsNullOrEmpty(category))
            {
                jqfdto = FindNeededFacet("jobcategory", jobSearchResultDto.Facets);


                List<DisplayItem> Categories = new List<DisplayItem>();

                if (jqfdto?.Facets == null)
                    return RedirectPermanent(Request.Path + "/1");

                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Categories.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label} ({FacetItem.Count})",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}/1"
                    });
                }
                return View("BrowseByType", new BrowseJobsLocationViewModel() { Items = Categories });

            }

            return NotFound();

            
            
        }

        #region Browse job by location helpers
        private void DeterminePaginationRange(ref BrowseJobsLocationViewModel Model)
        {
            int? CurrentPage = Model.CurrentPage;
            int NumberOfPages = Model.NumberOfPages;

            // Base case when there are less than 5 pages of results returned
            if(NumberOfPages <= 5)
            {
                Model.PaginationRangeLow = 1;
                Model.PaginationRangeHigh = NumberOfPages;
                return;
            }
            
            // Base case when the current page is one of first two pages
            if(CurrentPage < 3)
            {
                Model.PaginationRangeLow = 1;
                Model.PaginationRangeHigh = 5;
                return;
            }

            // Base case for when current page is one of last two pages
            if(CurrentPage > (NumberOfPages - 2))
            {
                Model.PaginationRangeLow = NumberOfPages - 4;
                Model.PaginationRangeHigh = NumberOfPages;
                return;
            }

            // Last case for when current page is somewhere in the middle of a result set
            // of greater than 5 pages.
            Model.PaginationRangeLow = (int)CurrentPage - 2;
            Model.PaginationRangeHigh = (int)CurrentPage + 2;
        }


        private string AssembleBaseUrl(
            string country = null,
            string state = null,
            string city = null,
            string industry = null,
            string category = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/browse-jobs-location" + 
                (country == null ? string.Empty : "/" + country) +
                (state == null ? string.Empty : "/" + state) +
                (city == null ? string.Empty : "/" + city) +
                (industry == null ? string.Empty : "/" + industry) +
                (category == null ? string.Empty : "/" + category));
            return sb.ToString();
        }

        private bool IsValidUrl(
            JobSearchResultDto SearchResult, 
            string country,
            string state,
            string city,
            string industry,
            string category)
        {
            if (!country.ToLower().Equals("us"))
                return false;

            if (!string.IsNullOrEmpty(state))
            {
                JobQueryFacetDto StateFacet = FindNeededFacet("admin_1", SearchResult.Facets);
                if (StateFacet == null)
                    return false;

                if (state.Length == 2)
                {
                    if (!FacetLabelExists(StateFacet.Facets, state))
                        return false;
                }
                else
                {
                    try
                    {
                        /**
                         * It's possible for a user to enter either "MD" or "Maryland", so we need to check either input
                         * against the returned facet, which will be the two-character state code (e.g. "md").
                         */
                        if (!FacetLabelExists(StateFacet.Facets, UpDiddyLib.Helpers.Utils.GetStateByName(state.Replace("-", " ")).ToString()))
                            return false;
                    }
                    catch (Exception e)
                    {
                        // Returns false if unable to find matching state from
                        return false;
                    }
                }
                
            }

            if (!string.IsNullOrEmpty(city))
            {
                JobQueryFacetDto CityFacet = FindNeededFacet("city", SearchResult.Facets);
                if (CityFacet == null)
                    return false;
            }

            if (!string.IsNullOrEmpty(industry))
            {
                JobQueryFacetDto IndustryFacet = FindNeededFacet("industry", SearchResult.Facets);
                if (IndustryFacet == null)
                    return false;
            }

            if (!string.IsNullOrEmpty(category))
            {
                JobQueryFacetDto CategoryFacet = FindNeededFacet("jobcategory", SearchResult.Facets);
                if (CategoryFacet == null)
                    return false;
            }


            return true;
            
        }

        private bool FacetLabelExists(List<JobQueryFacetItemDto> List, string Label)
        {
            foreach (JobQueryFacetItemDto Item in List)
            {
                if (Item.Label.ToLower().Replace(" ", "-").Equals(Label.ToLower()))
                    return true;
            }
            return false;
        }

        private JobQueryFacetDto FindNeededFacet(string key, List<JobQueryFacetDto> List)
        {
            foreach(JobQueryFacetDto facet in List)
            {
                if (facet.Name.ToLower().Equals(key.ToLower()))
                    return facet;
            }

            return null;
        }
        

        #endregion

    }
}
