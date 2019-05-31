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
using System.Security.Claims;


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
        public async Task<IActionResult> Index()
        {
            //get pageCount from Configuration file
            int pageCount = _configuration.GetValue<int>("Pagination:PageCount");

            JobSearchResultDto jobSearchResultDto = null;
            Dictionary<Guid, Guid> favoritesMap = new Dictionary<Guid, Guid>();

            var queryParametersString = Request.QueryString.ToString();
            // get Query String Parameters
            if (string.IsNullOrEmpty(queryParametersString) || string.IsNullOrWhiteSpace(queryParametersString))
            {
                queryParametersString += "?";
            }
            else
            {
                queryParametersString += "&";
            }

            string Keywords = Request.Query["keywords"];
            string Location = Request.Query["location"];

            if (Keywords != null)
                Keywords = Keywords.Trim();

            if (Location != null)
                Location = Location.Trim();


            ViewBag.QueryUrl = Request.Path + queryParametersString;

            int.TryParse(Request.Query["page"], out int page);

            try
            {
                jobSearchResultDto = await _api.GetJobsByLocation(
                                      queryParametersString);

                if (User.Identity.IsAuthenticated)
                    favoritesMap = await _api.JobFavoritesByJobGuidAsync(jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount).Select(job => job.JobPostingGuid).ToList());

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



            JobSearchViewModel jobSearchViewModel = new JobSearchViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount),
                FavoritesMap = favoritesMap,
                Facets = jobSearchResultDto.Facets,
                Keywords = Keywords,
                Location = Location
            };

            return View("Index", jobSearchViewModel);
        }

        [HttpGet]
        [Route("jobs/{JobGuid}")]
        [Route("jobs/{industry}/{category}/{country}/{state}/{city}/{JobGuid}")]
        public IActionResult RedirectOldJobsUrl(Guid JobGuid)
        {
            return RedirectToActionPermanent("JobAsync", "Jobs", new { JobGuid = JobGuid });
        }

        [HttpGet]
        [Route("job/{JobGuid}")]
        [Route("job/{industry}/{category}/{country}/{state}/{city}/{JobGuid}")]
        public async Task<IActionResult> JobAsync(Guid JobGuid)
        {
            JobPostingDto job = null;
            try
            {
                job = await _api.GetJobAsync(JobGuid, GoogleCloudEventsTrackingDto.Build(HttpContext.Request.Query, UpDiddyLib.Shared.GoogleJobs.ClientEventType.View));
                if (job.JobStatus == (int)JobPostingStatus.Draft)
                {
                    BasicResponseDto ResponseDto = new BasicResponseDto() { StatusCode = 401, Description = "Draft jobs cannot be viewed" };
                    throw new ApiException(new System.Net.Http.HttpResponseMessage(), ResponseDto);
                }


            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (404):
                        // Try to find as an expired job for representatives search.
                        //todo: See if we can allow expired/deleted jobs to get here to decide to show representatives and save an API call.
                        try
                        {
                            job = await _api.GetExpiredJobAsync(JobGuid);
                        }
                        catch (ApiException ae)
                        {
                            return StatusCode(ae.ResponseDto.StatusCode);
                        }

                        int pageCount = _configuration.GetValue<int>("Pagination:PageCount");
                        string location = string.Empty;
                        JobSearchResultDto jobSearchResultDto;

                        if (job is null)
                        {
                            // Show all jobs.
                            jobSearchResultDto = await _api.GetJobsByLocation(null);
                        }
                        else
                        {

                            // Show representatives.
                            location = job?.City + ", " + job?.Province;
                            var queryParametersString = $"?{job.Title}&{location}";

                            jobSearchResultDto = await _api.GetJobsByLocation(queryParametersString);

                            jobSearchResultDto = await _api.GetJobsByLocation(queryParametersString);
                        }

                        if (jobSearchResultDto == null)
                            return NotFound();

                        var jobSearchViewModel = new JobSearchViewModel()
                        {
                            Keywords = job?.Title ?? string.Empty,
                            Location = location,
                            JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount)
                        };

                        // Remove the expired job link from the search provider's index.
                        Response.StatusCode = 404;

                        ViewBag.QueryUrl = string.Empty;
                        return View("Index", jobSearchViewModel);
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

            Guid? jobFavoriteGuid = null;
            if (User.Identity.IsAuthenticated)
            {
                var favorite = await _api.JobFavoritesByJobGuidAsync(new List<Guid>() { job.JobPostingGuid.Value });
                if (favorite.Any())
                    jobFavoriteGuid = favorite.First().Value;
            }

            JobDetailsViewModel jdvm = new JobDetailsViewModel
            {
                RequestId = job.RequestId,
                ClientEventId = job.ClientEventId,
                JobPostingFavoriteGuid = jobFavoriteGuid,
                Name = job.Title,
                JobPostingGuid = job.JobPostingGuid,
                Company = job.Company?.CompanyName,
                PostedDate = job.PostingDateUTC == null ? string.Empty : job.PostingDateUTC.ToLocalTime().ToString(),
                Location = $"{job.City}, {job.Province}, {job.Country}",
                PostingId = job.JobPostingGuid?.ToString(),
                EmployeeType = job.EmploymentType?.Name,
                Summary = job.Description,
                ThirdPartyIdentifier = job.ThirdPartyIdentifier,
                CompanyBoilerplate = job.Company.JobPageBoilerplate,
                IsThirdPartyJob = job.ThirdPartyApply
            };

            // Display subscriber info if it exists
            if (job.Recruiter.Subscriber != null)
            {
                jdvm.ContactEmail = job.Recruiter.Subscriber?.Email;
                jdvm.ContactName = string.Join(' ',
                    new[] {
                        job.Recruiter.Subscriber?.FirstName,
                        job.Recruiter.Subscriber?.LastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                );
                jdvm.ContactPhone = job.Recruiter.Subscriber?.PhoneNumber;
            }
            else // Use recruiter info in no subscriber exists
            {
                jdvm.ContactEmail = job.Recruiter?.Email;
                jdvm.ContactName = string.Join(' ',
                    new[] {
                        job.Recruiter?.FirstName,
                        job.Recruiter?.LastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                );
                jdvm.ContactPhone = job.Recruiter?.PhoneNumber;
            }

            //check if user is logged in
            if (this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value != null)
            {
                var subscriber = await _api.SubscriberAsync(Guid.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value), false);
                jdvm.LoggedInSubscriberGuid = subscriber.SubscriberGuid;
                jdvm.LoggedInSubscriberEmail = subscriber.Email;
                jdvm.LoggedInSubscriberName = subscriber.FirstName + " " + subscriber.LastName;
            }

            //update job as viewed if there is referrer code
            if (Request.Cookies["referrerCode"] != null)
                await _Api.UpdateJobViewed(Request.Cookies["referrerCode"].ToString());

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
            return View("Apply", new JobApplicationViewModel()
            {
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

                await _api.RecordClientEventAsync(JobApplicationViewModel.JobPostingGuid, new GoogleCloudEventsTrackingDto()
                {
                    RequestId = JobApplicationViewModel.RequestId,
                    ParentClientEventId = JobApplicationViewModel.ClientEventId,
                    Type = UpDiddyLib.Shared.GoogleJobs.ClientEventType.Application_Finish
                });

                cjavm.JobApplicationStatus = CompletedJobApplicationViewModel.ApplicationStatus.Success;
            }
            catch (ApiException e)
            {
                cjavm.JobApplicationStatus = CompletedJobApplicationViewModel.ApplicationStatus.Failed;
                cjavm.Description = e.ResponseDto.Description;
            }


            return View("Finish", cjavm);
        }

        [HttpGet("Browse-Jobs")]
        public async Task<IActionResult> BrowseAsync()
        {

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _api.GetJobsUsingRoute();
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


            BrowseJobsViewModel bjvm = new BrowseJobsViewModel();
            bjvm.ViewModels = new List<BrowseJobsByTypeViewModel>
            {
                GetStateViewModel(jobSearchResultDto.Facets, "/browse-jobs-location/us", "Select Desired State", true)
                //GetIndustryViewModel(jobSearchResultDto.Facets, "/browse-jobs-industry", "Select Desired Industry", true),
                //GetCategoryViewModel(jobSearchResultDto.Facets, "/browse-jobs-category", false, "Select Desired Category", true)
            };

            return View("Browse", bjvm);
        }


        [HttpGet("browse-jobs-location/{country?}")]
        [HttpGet("browse-jobs-location/{country}/{page:int?}")]
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
                jobSearchResultDto = await _api.GetJobsUsingRoute(
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

            BrowseJobsByTypeViewModel jobSearchViewModel = new BrowseJobsByTypeViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount),
                CurrentPage = page,
                NumberOfPages = jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0)
            };

            if (User.Identity.IsAuthenticated)
            {
                jobSearchViewModel.FavoritesMap = await _api.JobFavoritesByJobGuidAsync(jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount).Select(job => job.JobPostingGuid).ToList());
            }

            // Google seems to be capping the number of results at 500, so we account for that here.
            if (jobSearchViewModel.NumberOfPages > 500)
                jobSearchViewModel.NumberOfPages = 500;

            DeterminePaginationRange(ref jobSearchViewModel);

            // User has reached the end of the browse flow, so present results.
            if (!string.IsNullOrEmpty(category) || page != 0)
            {
                if (string.IsNullOrEmpty(state))
                    jobSearchViewModel.Header = "United States";
                else if (string.IsNullOrEmpty(city))
                {
                    string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                    jobSearchViewModel.Header = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel);
                }
                else if (string.IsNullOrEmpty(industry))
                    jobSearchViewModel.Header = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                else if (string.IsNullOrEmpty(category))
                    jobSearchViewModel.Header = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                else if (!string.IsNullOrEmpty(category))
                    jobSearchViewModel.Header = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;





                jobSearchViewModel.BaseUrl = AssembleBaseLocationUrl(country, state, city, industry, category);
                return View("BrowseByType", jobSearchViewModel);
            }



            // Return state view if user has only specified country
            if (string.IsNullOrEmpty(state))
            {
                BrowseJobsByTypeViewModel bjbtvm = GetStateViewModel(jobSearchResultDto.Facets, Request.Path, "Select Desired State");
                return View("BrowseByType", bjbtvm);
            }

            // If flow reaches this point, user has specified country and state, but 
            // needs to choose city
            if (string.IsNullOrEmpty(city))
            {

                JobQueryFacetDto jqfdto = FindNeededFacet("city", jobSearchResultDto.Facets);

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
                        Label = $"{FacetItem.Label.Split(",")[0]}",
                        Url = $"{Request.Path}/{rgx.Replace(FacetItem.Label.Split(",")[0].ToLower(), "-")}",
                        Count = $"{FacetItem.Count}"
                    });
                }

                string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                BrowseJobsByTypeViewModel bjlvm = new BrowseJobsByTypeViewModel()
                {
                    Items = LocationsCities,
                    Header = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel)
                };

                return View("BrowseByType", bjlvm);
            }

            if (string.IsNullOrEmpty(industry))
            {
                string CityLabel = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                BrowseJobsByTypeViewModel bjbtvm = GetIndustryViewModel(jobSearchResultDto.Facets, Request.Path, CityLabel);
                if (bjbtvm == null)
                    return RedirectPermanent(Request.Path + "/1");
                return View("BrowseByType", bjbtvm);
            }

            if (string.IsNullOrEmpty(category))
            {
                string IndustryLabel = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                BrowseJobsByTypeViewModel bjbtvm = GetCategoryViewModel(jobSearchResultDto.Facets, Request.Path, true, IndustryLabel);
                if (bjbtvm == null)
                    return RedirectPermanent(Request.Path + "/1");
                return View("BrowseByType", bjbtvm);

            }

            return NotFound();



        }

        public BrowseJobsByTypeViewModel GetStateViewModel(List<JobQueryFacetDto> Facets, string Path, string Header, bool HideAllLink = false)
        {
            JobQueryFacetDto jqfdto = FindNeededFacet("admin_1", Facets);
            List<DisplayItem> StateLocations = new List<DisplayItem>();
            jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
            foreach (JobQueryFacetItemDto JobQueryFacet in jqfdto.Facets)
            {
                UpDiddyLib.Helpers.Utils.State State;
                Enum.TryParse(JobQueryFacet.Label.ToUpper(), out State);
                string StateName = UpDiddyLib.Helpers.Utils.GetState(State);
                StateLocations.Add(new DisplayItem
                {
                    Label = $"{UpDiddyLib.Helpers.Utils.ToTitleCase(StateName)}",
                    Url = $"{Path}/{JobQueryFacet.Label.ToLower()}",
                    Count = $"{JobQueryFacet.Count}"
                });
            }

            BrowseJobsByTypeViewModel bjbtvm = new BrowseJobsByTypeViewModel()
            {
                Items = StateLocations,
                Header = "Select Desired State:",
                HideAllLink = HideAllLink
            };
            return bjbtvm;
        }

        public BrowseJobsByTypeViewModel GetIndustryViewModel(List<JobQueryFacetDto> Facets, string Path, string Header, bool HideAllLink = false)
        {
            JobQueryFacetDto jqfdto = FindNeededFacet("industry", Facets);

            if (jqfdto == null)
                return null;

            List<DisplayItem> Industries = new List<DisplayItem>();

            foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
            {
                Industries.Add(new DisplayItem
                {
                    Label = $"{FacetItem.Label}",
                    Url = $"{Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                    Count = $"{FacetItem.Count}"
                });
            }

            return new BrowseJobsByTypeViewModel() { Items = Industries, Header = Header, HideAllLink = HideAllLink };
        }

        public BrowseJobsByTypeViewModel GetCategoryViewModel(List<JobQueryFacetDto> Facets, string Path, bool ShowResults, string Header, bool HideAllLink = false)
        {
            JobQueryFacetDto jqfdto = FindNeededFacet("jobcategory", Facets);

            if (jqfdto == null)
                return null;

            List<DisplayItem> Categories = new List<DisplayItem>();
            foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
            {
                Categories.Add(new DisplayItem
                {
                    Label = $"{FacetItem.Label}",
                    Url = $"{Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}" + (ShowResults ? "/1" : string.Empty),
                    Count = $"{FacetItem.Count}"
                });
            }
            return new BrowseJobsByTypeViewModel() { Items = Categories, Header = Header, HideAllLink = HideAllLink };
        }


        #region Browse job by location helpers
        private void DeterminePaginationRange(ref BrowseJobsByTypeViewModel Model)
        {
            int? CurrentPage = Model.CurrentPage;
            int NumberOfPages = Model.NumberOfPages;

            // Base case when there are less than 5 pages of results returned
            if (NumberOfPages <= 3)
            {
                Model.PaginationRangeLow = 1;
                Model.PaginationRangeHigh = NumberOfPages;
                return;
            }

            // Base case when the current page is one of first two pages
            if (CurrentPage < 2)
            {
                Model.PaginationRangeLow = 1;
                Model.PaginationRangeHigh = 3;
                return;
            }

            // Base case for when current page is one of last two pages
            if (CurrentPage > (NumberOfPages - 1))
            {
                Model.PaginationRangeLow = NumberOfPages - 2;
                Model.PaginationRangeHigh = NumberOfPages;
                return;
            }

            // Last case for when current page is somewhere in the middle of a result set
            // of greater than 5 pages.
            Model.PaginationRangeLow = (int)CurrentPage - 1;
            Model.PaginationRangeHigh = (int)CurrentPage + 1;
        }


        private string AssembleBaseLocationUrl(
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

        private string AssembleBaseIndustryUrl(
            string industry = null,
            string category = null,
            string country = null,
            string state = null,
            string city = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/browse-jobs-industry" +

                (industry == null ? string.Empty : "/" + industry) +
                (category == null ? string.Empty : "/" + category) +
                (country == null ? string.Empty : "/" + country) +
                (state == null ? string.Empty : "/" + state) +
                (city == null ? string.Empty : "/" + city));
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
            if (!string.IsNullOrEmpty(country) && !country.ToLower().Equals("us"))
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
                if (CityFacet == null || !FacetLabelExists(CityFacet.Facets, city))
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
                if (Regex.Replace(Item.Label.Split(",")[0].ToLower(), @"\.| |'", "-").Equals(Label.ToLower()))
                    return true;
            }
            return false;
        }

        private JobQueryFacetDto FindNeededFacet(string key, List<JobQueryFacetDto> List)
        {
            foreach (JobQueryFacetDto facet in List)
            {
                if (facet.Name.ToLower().Equals(key.ToLower()))
                    return facet;
            }

            return null;
        }

        private string StateCodeToFullName(string stateCode)
        {
            // The following uses the C# inline out function, which initializes
            // the variable for the local scope.
            Enum.TryParse(stateCode.ToUpper(), out UpDiddyLib.Helpers.Utils.State state);
            return UpDiddyLib.Helpers.Utils.GetState(state);
        }


        #endregion

        [HttpGet("browse-jobs-industry/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry?}")]
        [HttpGet("browse-jobs-industry/{industry}/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry}/{category?}/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry}/{category}/{country?}/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry}/{category}/{country}/{state?}/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry}/{category}/{country}/{state}/{city?}/{page:int?}")]
        public async Task<IActionResult> BrowseJobsByIndustryAsync(
           string industry,
           string category,
           string country,
           string state,
           string city,
           int page)
        {




            if (!string.IsNullOrEmpty(industry) &&
                !string.IsNullOrEmpty(category) &&
                !string.IsNullOrEmpty(country) &&
                !string.IsNullOrEmpty(state) &&
                !string.IsNullOrEmpty(city) &&
                page == 0)
                return RedirectPermanent($"{Request.Path}/1");

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _api.GetJobsUsingRoute(
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

            BrowseJobsByTypeViewModel jobSearchViewModel = new BrowseJobsByTypeViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount),
                CurrentPage = page,
                NumberOfPages = jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0)
            };

            if (User.Identity.IsAuthenticated)
            {
                jobSearchViewModel.FavoritesMap = await _api.JobFavoritesByJobGuidAsync(jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount).Select(job => job.JobPostingGuid).ToList());
            }

            // Google seems to be capping the number of results at 500, so we account for that here.
            if (jobSearchViewModel.NumberOfPages > 500)
                jobSearchViewModel.NumberOfPages = 500;

            DeterminePaginationRange(ref jobSearchViewModel);

            // User has reached the end of the browse flow, so present results.
            if (!string.IsNullOrEmpty(city) || page != 0)
            {
                if (string.IsNullOrEmpty(industry))
                    jobSearchViewModel.Header = "All";
                else if (string.IsNullOrEmpty(category))
                    jobSearchViewModel.Header = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                else if (string.IsNullOrEmpty(state))
                    jobSearchViewModel.Header = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                else if (string.IsNullOrEmpty(city))
                {
                    string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                    jobSearchViewModel.Header = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel);
                }
                else if (!string.IsNullOrEmpty(city))
                    jobSearchViewModel.Header = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                jobSearchViewModel.BaseUrl = AssembleBaseIndustryUrl(country, state, city, industry, category);
                return View("BrowseByType", jobSearchViewModel);
            }

            JobQueryFacetDto jqfdto = FindNeededFacet("industry", jobSearchResultDto.Facets);

            if (string.IsNullOrEmpty(industry))
            {

                List<DisplayItem> Industries = new List<DisplayItem>();


                if (jqfdto?.Facets == null)
                    return RedirectPermanent(Request.Path + "/1");


                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Industries.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label}",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                        Count = $"{FacetItem.Count}"
                    });
                }
                return View("BrowseByType", new BrowseJobsByTypeViewModel() { Items = Industries, Header = "Select Desired Industry:" });
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
                        Label = $"{FacetItem.Label}",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                        Count = $"{FacetItem.Count}"
                    });
                }
                string IndustryLabel = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                return View("BrowseByType", new BrowseJobsByTypeViewModel() { Items = Categories, Header = IndustryLabel });

            }




            // Return state view if user has only specified country
            if (string.IsNullOrEmpty(state))
            {
                jqfdto = FindNeededFacet("admin_1", jobSearchResultDto.Facets);
                List<DisplayItem> StateLocations = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto JobQueryFacet in jqfdto.Facets)
                {
                    UpDiddyLib.Helpers.Utils.State State;
                    Enum.TryParse(JobQueryFacet.Label.ToUpper(), out State);
                    string StateName = UpDiddyLib.Helpers.Utils.GetState(State);
                    StateLocations.Add(new DisplayItem
                    {
                        Label = $"{UpDiddyLib.Helpers.Utils.ToTitleCase(StateName)}",
                        Url = $"{Request.Path}/us/{JobQueryFacet.Label.ToLower()}",
                        Count = $"{JobQueryFacet.Count}"
                    });
                }
                string CategoryLabel = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                BrowseJobsByTypeViewModel bjlvmState = new BrowseJobsByTypeViewModel()
                {
                    Items = StateLocations,
                    Header = CategoryLabel
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
                        Label = $"{FacetItem.Label.Split(",")[0]}",
                        Url = $"{Request.Path}/{rgx.Replace(FacetItem.Label.Split(",")[0].ToLower(), "-")}/1",
                        Count = $"{FacetItem.Count}"
                    });
                }
                string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                BrowseJobsByTypeViewModel bjlvm = new BrowseJobsByTypeViewModel()
                {
                    Items = LocationsCities,
                    Header = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel)
                };

                return View("BrowseByType", bjlvm);
            }





            return NotFound();



        }

        [HttpGet("browse-jobs-category/{page:int?}")]
        [HttpGet("browse-jobs-category/{category?}")]
        [HttpGet("browse-jobs-category/{category}/{page:int?}")]
        [HttpGet("browse-jobs-category/{category}/{industry?}/{page:int?}")]
        [HttpGet("browse-jobs-category/{category}/{industry}/{country?}/{page:int?}")]
        [HttpGet("browse-jobs-category/{category}/{industry}/{country}/{state?}/{page:int?}")]
        [HttpGet("browse-jobs-category/{category}/{industry}/{country}/{state}/{city?}/{page:int?}")]
        public async Task<IActionResult> BrowseJobsByCategoryAsync(
           string category,
           string industry,
           string country,
           string state,
           string city,
           int page)
        {




            if (!string.IsNullOrEmpty(industry) &&
                !string.IsNullOrEmpty(category) &&
                !string.IsNullOrEmpty(country) &&
                !string.IsNullOrEmpty(state) &&
                !string.IsNullOrEmpty(city) &&
                page == 0)
                return RedirectPermanent($"{Request.Path}/1");

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _api.GetJobsUsingRoute(
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

            BrowseJobsByTypeViewModel jobSearchViewModel = new BrowseJobsByTypeViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount),
                CurrentPage = page,
                NumberOfPages = jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0)
            };

            if (User.Identity.IsAuthenticated)
            {
                jobSearchViewModel.FavoritesMap = await _api.JobFavoritesByJobGuidAsync(jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount).Select(job => job.JobPostingGuid).ToList());
            }

            // Google seems to be capping the number of results at 500, so we account for that here.
            if (jobSearchViewModel.NumberOfPages > 500)
                jobSearchViewModel.NumberOfPages = 500;

            DeterminePaginationRange(ref jobSearchViewModel);

            // User has reached the end of the browse flow, so present results.
            if (!string.IsNullOrEmpty(city) || page != 0)
            {
                if (string.IsNullOrEmpty(category))
                    jobSearchViewModel.Header = "All";
                else if (string.IsNullOrEmpty(industry))
                    jobSearchViewModel.Header = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                else if (string.IsNullOrEmpty(state))
                    jobSearchViewModel.Header = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                else if (string.IsNullOrEmpty(city))
                {
                    string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                    jobSearchViewModel.Header = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel);
                }
                else if (!string.IsNullOrEmpty(city))
                    jobSearchViewModel.Header = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                jobSearchViewModel.BaseUrl = AssembleBaseIndustryUrl(country, state, city, industry, category);
                return View("BrowseByType", jobSearchViewModel);
            }

            JobQueryFacetDto jqfdto = FindNeededFacet("jobcategory", jobSearchResultDto.Facets);


            if (string.IsNullOrEmpty(category))
            {



                List<DisplayItem> Categories = new List<DisplayItem>();

                if (jqfdto?.Facets == null)
                    return RedirectPermanent(Request.Path + "/1");

                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Categories.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label}",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                        Count = $"{FacetItem.Count}"
                    });
                }
                return View("BrowseByType", new BrowseJobsByTypeViewModel() { Items = Categories, Header = "Select Desired Category:" });

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
                        Label = $"{FacetItem.Label}",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                        Count = $"{FacetItem.Count}"
                    });
                }
                string JobCategoryLabel = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                return View("BrowseByType", new BrowseJobsByTypeViewModel() { Items = Industries, Header = JobCategoryLabel });
            }






            // Return state view if user has only specified country
            if (string.IsNullOrEmpty(state))
            {
                jqfdto = FindNeededFacet("admin_1", jobSearchResultDto.Facets);
                List<DisplayItem> StateLocations = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto JobQueryFacet in jqfdto.Facets)
                {
                    UpDiddyLib.Helpers.Utils.State State;
                    Enum.TryParse(JobQueryFacet.Label.ToUpper(), out State);
                    string StateName = UpDiddyLib.Helpers.Utils.GetState(State);
                    StateLocations.Add(new DisplayItem
                    {
                        Label = $"{UpDiddyLib.Helpers.Utils.ToTitleCase(StateName)}",
                        Url = $"{Request.Path}/us/{JobQueryFacet.Label.ToLower()}",
                        Count = $"{JobQueryFacet.Count}"
                    });
                }
                string IndustryLabel = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                BrowseJobsByTypeViewModel bjlvmState = new BrowseJobsByTypeViewModel()
                {
                    Items = StateLocations,
                    Header = IndustryLabel
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
                        Label = $"{FacetItem.Label.Split(",")[0]}",
                        Url = $"{Request.Path}/{rgx.Replace(FacetItem.Label.Split(",")[0].ToLower(), "-")}/1",
                        Count = $"{FacetItem.Count}"
                    });
                }
                string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                BrowseJobsByTypeViewModel bjlvm = new BrowseJobsByTypeViewModel()
                {
                    Items = LocationsCities,
                    Header = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel)
                };

                return View("BrowseByType", bjlvm);
            }





            return NotFound();



        }

        [Authorize]
        [HttpPost]
        [Route("[controller]/ReferAJob", Name = "ReferJobToFriend")]
        public async Task<IActionResult> ReferAJob(string jobPostingId, string referrerGuid, string refereeName, string refereeEmailId, string descriptionEmailBody)
        {
            //send email to referree for the job posting
            await _api.ReferJobPosting(jobPostingId, referrerGuid, refereeName, refereeEmailId, descriptionEmailBody);

            Guid jobPostingGuid = Guid.Parse(jobPostingId);
            return await JobAsync(jobPostingGuid);
        }
    }
}
