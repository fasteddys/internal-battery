using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using UpDiddyApi.Helpers.Job;
using System.Security.Claims;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.Controllers
{

    public class JobController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30;
        private readonly ICloudTalentService _cloudTalentService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IServiceProvider _services;
        private readonly IJobService _jobService;
        private readonly IHangfireService _hangfireService;
        private readonly ISubscriberService _subscriberService;
        private readonly IJobPostingService _jobPostingService;

        #region constructor 
        public JobController(IServiceProvider services, IHangfireService hangfireService, ICloudTalentService cloudTalentService)

        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _subscriberService = _services.GetService<ISubscriberService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalentService = cloudTalentService;

            //job Service to perform all business logic related to jobs
            _jobService = _services.GetService<IJobService>();
            _jobPostingService = _services.GetService<IJobPostingService>();
            _hangfireService = hangfireService;
        }

        #endregion

        #region job statistics 


        /// <summary>
        /// Get all job postings for a subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/scrape-statistics/{numRecords}")]
        public IActionResult GetJobsForSubscriber(int numRecords)
        {
            var stats = _repositoryWrapper.JobSiteScrapeStatistic.GetJobScrapeStatisticsAsync(numRecords).Result;
            return Ok(_mapper.Map<List<JobSiteScrapeStatisticDto>>(stats));
        }


        #endregion

        #region job posting favorites


        [HttpGet]
        [Authorize]
        [Route("api/[controller]/favorite/subscriber/{subscriberGuid}")]
        public async Task<IActionResult> GetJobFavoritesForSubscriber(Guid subscriberGuid)
        {

            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subscriberGuid == null || subscriberGuid != subsriberGuidClaim)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Job Posting Favorites can only be viewed by their owner" });

            List<JobPosting> subscriberJobPostingFavorites = await JobPostingFavoriteFactory.GetJobPostingFavoritesForSubscriber(_repositoryWrapper, subscriberGuid);
            return Ok(_mapper.Map<List<JobPostingDto>>(subscriberJobPostingFavorites));
        }



        [HttpDelete]
        [Authorize]
        [Route("api/[controller]/favorite/{jobPostingFavoriteGuid}")]
        public async Task<IActionResult> DeleteJobPostingFavorite(Guid jobPostingFavoriteGuid)
        {
            JobPostingFavorite jobPostingFavorite = null;
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPostingFavorite started at: {DateTime.UtcNow.ToLongDateString()} for posting {jobPostingFavoriteGuid}");

                if (jobPostingFavoriteGuid == null)
                    return BadRequest(new { code = 400, message = "No job posting favorite identifier was provided" });

                jobPostingFavorite = await JobPostingFavoriteFactory.GetJobPostingFavoriteByGuidWithRelatedObjects(_repositoryWrapper, jobPostingFavoriteGuid);
                if (jobPostingFavorite == null)
                    return NotFound(new { code = 404, message = $"Job posting favorite {jobPostingFavoriteGuid} does not exist" });

                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPostingFavorite.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Can only delete your own job posting favorites" });

                jobPostingFavorite.IsDeleted = 1;
                jobPostingFavorite.ModifyDate = DateTime.UtcNow;

                _db.SaveChanges();

                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPostingFavorite completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPostingFavorite exception : {ex.Message} while deleting posting {jobPostingFavoriteGuid}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
            return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"JobPosting {jobPostingFavorite.JobPostingFavoriteGuid}  has been deleted " });
        }



        [HttpPost]
        [Authorize]
        [Route("api/[controller]/favorite")]
        public IActionResult CreateJobPostingFavorite([FromBody] JobPostingFavoriteDto jobPostingFavoriteDto)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:JobPostingFavoriteDto started at: {DateTime.UtcNow.ToLongDateString()}");
                if (jobPostingFavoriteDto == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Job posting favorite required" });

                // Validate request 
                Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                JobPosting jobPosting = null;
                Subscriber subscriber = null;
                string ErrorMsg = string.Empty;
                if (JobPostingFavoriteFactory.ValidateJobPostingFavorite(_repositoryWrapper, jobPostingFavoriteDto, subscriberGuid, ref subscriber, ref jobPosting, ref ErrorMsg) == false)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ErrorMsg });
                else
                {
                    JobPostingFavorite jobPostingFavorite = _db.JobPostingFavorite.Where(jpf => jpf.SubscriberId == subscriber.SubscriberId && jpf.JobPostingId == jobPosting.JobPostingId).FirstOrDefault();
                    if (jobPostingFavorite == null)
                    {
                        jobPostingFavorite = JobPostingFavoriteFactory.CreateJobPostingFavorite(subscriber, jobPosting);
                        _db.JobPostingFavorite.Add(jobPostingFavorite);
                    }
                    else
                    {
                        jobPostingFavorite.IsDeleted = 0;
                        jobPostingFavorite.ModifyDate = DateTime.UtcNow;
                    }

                    try
                    {
                        _db.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
                    }

                    _syslog.Log(LogLevel.Information, $"***** JobController:JobPostingFavoriteDto completed at: {DateTime.UtcNow.ToLongDateString()}");
                    return Ok(new JobPostingFavoriteDto() { JobPostingFavoriteGuid = jobPostingFavorite.JobPostingFavoriteGuid });
                }
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:JobPostingFavoriteDto exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }

        #endregion

        #region job crud 

        /// <summary>
        /// Get all job postings for a subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]/subscriber/{subscriberGuid}")]
        public async Task<IActionResult> GetJobsForSubscriber(Guid subscriberGuid)
        {


            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subscriberGuid == null || subscriberGuid != subsriberGuidClaim)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting can only be viewed by their owner" });

            List<JobPosting> jobPostings = await JobPostingFactory.GetJobPostingsForSubscriber(_repositoryWrapper, subscriberGuid);

            return Ok(_mapper.Map<List<JobPostingDto>>(jobPostings));
        }


        /// <summary>
        /// Delete a job posting 
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]/{jobPostingGuid}")]
        public IActionResult DeleteJobPosting(Guid jobPostingGuid)
        {

            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting started at: {DateTime.UtcNow.ToLongDateString()} for posting {jobPostingGuid}");

                if (jobPostingGuid == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "No job posting identifier was provided" });

                string ErrorMsg = string.Empty;
                if (JobPostingFactory.DeleteJob(_repositoryWrapper, jobPostingGuid, ref ErrorMsg, _syslog, _mapper, _configuration, _hangfireService) == false)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ErrorMsg });
                else
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"JobPosting {jobPostingGuid}  has been deleted " });
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting exception : {ex.Message} while deleting posting {jobPostingGuid}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }

        /// <summary>
        /// Update a job posting 
        /// </summary>
        /// <param name="jobPostingDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]")]
        public IActionResult UpdateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {
            try
            {
                // validate request 
                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPostingDto.Recruiter.Subscriber == null || jobPostingDto.Recruiter.Subscriber.SubscriberGuid == null || jobPostingDto.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting started at: {DateTime.UtcNow.ToLongDateString()}");
                // update the job posting 
                string ErrorMsg = string.Empty;
                bool UpdateOk = JobPostingFactory.UpdateJobPosting(_repositoryWrapper, jobPostingDto.JobPostingGuid.Value, jobPostingDto, ref ErrorMsg, _hangfireService);
                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
                if (UpdateOk)
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{jobPostingDto.JobPostingGuid.Value}" });
                else
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ErrorMsg });
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:UpdateJobPosting exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }


        /// <summary>
        /// Create a job posting 
        /// </summary>
        /// <param name="jobPostingDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]")]
        public async Task<IActionResult> CreateJobPosting([FromBody] JobPostingDto jobPostingDto)
        {
            try
            {
                // Validate request 
                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobPostingDto.Recruiter.Subscriber == null || jobPostingDto.Recruiter.Subscriber.SubscriberGuid == null || jobPostingDto.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting owner is not specified or does not match user posting job" });

                if (jobPostingDto == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "JobPosting is required" });

                Recruiter recruiter = await RecruiterFactory.GetRecruiterBySubscriberGuid(_repositoryWrapper, jobPostingDto.Recruiter.Subscriber.SubscriberGuid.Value);

                if (recruiter == null)
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = $"Recruiter {jobPostingDto.Recruiter.Subscriber.SubscriberId} rec not found" });


                string errorMsg = string.Empty;
                Guid newPostingGuid = Guid.Empty;
                if (JobPostingFactory.PostJob(_repositoryWrapper, recruiter.RecruiterId, jobPostingDto, ref newPostingGuid, ref errorMsg, _syslog, _mapper, _configuration, _hangfireService) == true)
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{newPostingGuid}" });
                else
                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = errorMsg });

            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }

        /// <summary>
        /// Copy a job posting 
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]/{jobPostingGuid}")]
        public async Task<IActionResult> CopyJob(Guid jobPostingGuid)
        {
            // Get the posting to be copied as an untracked entity
            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                return NotFound(new BasicResponseDto() { StatusCode = 404, Description = "JobPosting not found" });

            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (jobPosting.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim)
                return BadRequest(new BasicResponseDto() { StatusCode = 401, Description = "Unauthorized to copy posting" });

            jobPosting = await JobPostingFactory.CopyJobPosting(_repositoryWrapper, jobPosting, _postingTTL);

            return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{jobPosting.JobPostingGuid}" });
        }

        #endregion

        #region job search 

        /// <summary>
        /// Retrieves unique values for the autocomplete feature of the keyword job search box.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/[controller]/keyword-search-terms")]
        public async Task<IActionResult> GetKeywordSearchTerms()
        {
            var keywordSearchTerms = await _jobService.GetKeywordSearchTermsAsync();
            return Ok(keywordSearchTerms);
        }

        /// <summary>
        /// Retrieves unique values for the autocomplete feature of the location job search box.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/[controller]/location-search-terms")]
        public async Task<IActionResult> GetLocationSearchTerms()
        {
            var locationSearchTerms = await _jobService.GetLocationSearchTermsAsync();
            return Ok(locationSearchTerms);
        }

        /// <summary>
        /// Get a specific job posting 
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/[controller]/{jobPostingGuid}")]
        public async Task<IActionResult> GetJob(Guid jobPostingGuid)
        {
            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuidWithRelatedObjectsAsync(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                return NotFound(new BasicResponseDto() { StatusCode = 404, Description = "JobPosting not found" });
            JobPostingDto rVal = _mapper.Map<JobPostingDto>(jobPosting);

            // set meta data for seo
            JobPostingFactory.SetMetaData(jobPosting, rVal);

            /* i would prefer to get the semantic url in automapper, but i ran into a blocker while trying to call the static util method
             * in "MapFrom" while guarding against null refs: an expression tree lambda may not contain a null propagating operator
             * .ForMember(jp => jp.SemanticUrl, opt => opt.MapFrom(src => Utils.GetSemanticJobUrlPath(src.Industry?.Name,"","","","","")))
             */

            rVal.SemanticJobPath = Utils.CreateSemanticJobPath(
                jobPosting.Industry?.Name,
                jobPosting.JobCategory?.Name,
                jobPosting.Country,
                jobPosting.Province,
                jobPosting.City,
                jobPostingGuid.ToString());

            JobQueryDto jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(jobPosting.Province, jobPosting.City, jobPosting.Title, Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]));
            JobSearchResultDto jobSearchForSingleJob = _cloudTalentService.JobSearch(jobQuery);

            // If jobs in same city come back less than 6, broaden search to state.
            if (jobSearchForSingleJob.JobCount < Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]))
            {
                jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(jobPosting.Province, string.Empty, jobPosting.Title, Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]));
                jobSearchForSingleJob = _cloudTalentService.JobSearch(jobQuery);
            }



            rVal.SimilarJobs = jobSearchForSingleJob;

            return Ok(rVal);
        }

        [HttpGet]
        [Route("api/[controller]/active-job-count")]
        public async Task<IActionResult> GetActiveJobCount()
        {
            var allJobPostings = _repositoryWrapper.JobPosting.GetAll().Where(jp => jp.IsDeleted == 0);
            var count = await allJobPostings.CountAsync();
            return Ok(new BasicResponseDto() { StatusCode = 200, Description = count.ToString() });
        }

        [HttpGet]
        [Route("api/sitemap/[controller]/")]
        public async Task<IActionResult> GetAllJobsForSitemap()
        {
            var allJobsForSitemap = await JobPostingFactory.GetAllJobPostingsForSitemap(_repositoryWrapper);
            return Ok(_mapper.Map<List<JobPostingDto>>(allJobsForSitemap));
        }

        [HttpGet]
        [Route("api/[controller]/browse-jobs-location/{Country?}/{Province?}/{City?}/{Industry?}/{JobCategory?}/{Skill?}/{PageNum?}")]
        public async Task<IActionResult> JobSearchByLocation(string Country, string Province, string City, string Industry, string JobCategory, string Skill, int PageNum)
        {           
            JobSearchResultDto rVal=null;
            try
            {
                if(ModelState.IsValid)
                {
                    rVal= await _jobService.GetJobsByLocationAsync(Country, Province, City, Industry, JobCategory, Skill, PageNum, Request.Query);
                }
                else
                {
                    _syslog.Log(LogLevel.Information, $"Invalid data for Country={Country}, Province={Province}, City={City}, Industry={Industry}, JobCategory={JobCategory}, Skill={Skill} and PageNumber={PageNum}");
                    return BadRequest();
                }
            }
            catch(Exception ex)
            {
                _syslog.Log(LogLevel.Error, "JobController.JobSearchByLocation:",ex.StackTrace);
                return StatusCode(500, ex.Message);
            }
            return Ok(rVal);
        }

        [HttpGet]
        [Route("api/[controller]/browse-jobs-industry/{Industry?}/{JobCategory?}/{Country?}/{Province?}/{City?}/{Skill?}/{PageNum?}")]
        public IActionResult JobSearchIndustry(string Industry, string JobCategory, string Country, string Province, string City, string Skill, int PageNum)
        {

            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobQueryDto jobQuery = JobQueryHelper.CreateJobQuery(Country, Province, City, Industry, JobCategory, Skill, PageNum, PageSize, Request.Query);
            JobSearchResultDto rVal = _cloudTalentService.JobSearch(jobQuery);
            return Ok(rVal);
        }


        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult JobSearch([FromBody] JobQueryDto jobQueryDto)
        {
            int PageSize = int.Parse(_configuration["CloudTalent:JobPageSize"]);
            JobSearchResultDto rVal = _cloudTalentService.JobSearch(jobQueryDto);
            return Ok(rVal);
        }

        /// <summary>
        /// Get a specific job posting for an expired job to use its context for comparables.
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/[controller]/expired/{jobPostingGuid}")]
        public async Task<IActionResult> GetExpiredJob(Guid jobPostingGuid)
        {
            JobPosting jobPosting = await JobPostingFactory.GetExpiredJobPostingByGuid(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                return NotFound(new BasicResponseDto() { StatusCode = 404, Description = "JobPosting not found" });

            return Ok(_mapper.Map<JobPostingDto>(jobPosting));
        }

        #endregion

        #region Google Cloud Talent Client Event Tracking/Report
        [HttpPost]
        [Route("api/[controller]/{jobGuid}/track")]
        public async Task<IActionResult> JobEvent(Guid jobGuid, [FromBody] GoogleCloudEventsTrackingDto dto)
        {
            JobPosting jp = await _db.JobPosting.Where(x => x.JobPostingGuid == jobGuid).FirstOrDefaultAsync();
            ClientEvent ce = await _cloudTalentService.CreateClientEventAsync(dto.RequestId, dto.Type, new List<string>() { jp.CloudTalentUri }, dto.ParentClientEventId);
            return Ok(new GoogleCloudEventsTrackingDto
            {
                RequestId = ce.RequestId,
                ClientEventId = ce.EventId
            });
        }
        #endregion

        #region Misc Job Utilities

        [HttpGet("api/[controller]/categories")]
        public async Task<IList<JobCategory>> GetJobCategories()
        {
            var result =  _repositoryWrapper.JobCategoryRepository.GetAll();
            return await result.ToListAsync();
        }

        [HttpGet("api/[controller]/post-count")]
        public async Task<IList<JobPostingCountDto>> GetJobCountPerProvinceAsync()
        {
            return await _jobPostingService.GetJobCountPerProvinceAsync();
        }

        #endregion

        #region Job Referral
        [HttpPost]
        [Authorize]
        [Route("api/[controller]/referral")]
        public async Task<IActionResult> JobReferral([FromBody]JobReferralDto jobReferralDto)
        {
            await _jobService.ReferJobToFriend(jobReferralDto);
            return Ok();
        }

        [HttpPut]
        [Authorize]
        [Route("api/[controller]/update-referral")]
        public async Task<IActionResult> UpdateReferral([FromBody]JobReferralDto jobReferralDto)
        {
            await _jobService.UpdateJobReferral(jobReferralDto.JobReferralGuid, jobReferralDto.RefereeGuid);
            return Ok();
        }

        [HttpPut]
        [Route("api/[controller]/update-job-viewed")]
        public async Task<IActionResult> UpdateJobViewed([FromBody]string referrerCode)
        {
            await _jobService.UpdateJobViewed(referrerCode);
            return Ok();
        }
        #endregion

        #region Job Alerts

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/alert")]
        public IActionResult CreateJobPostingAlert([FromBody] JobPostingAlertDto jobPostingAlertDto)
        {
            if (jobPostingAlertDto == null)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Missing required fields." });
            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subsriberGuidClaim == null || subsriberGuidClaim == Guid.Empty)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "You must be logged in to create a job alert." });
            else
                jobPostingAlertDto.Subscriber = new SubscriberDto() { SubscriberGuid = subsriberGuidClaim };
            var existingJobPostingAlerts = JobPostingAlertFactory.GetJobPostingAlertsBySubscriber(_repositoryWrapper, _syslog, jobPostingAlertDto.Subscriber.SubscriberGuid.Value);
            if (existingJobPostingAlerts != null && existingJobPostingAlerts.Count() == 5)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "You may only have 5 active job alerts - please delete one in 'My Alerts' first." });

            bool isSuccess = JobPostingAlertFactory.SaveJobPostingAlert(_repositoryWrapper, _syslog, jobPostingAlertDto, _hangfireService);

            if (isSuccess)
                return Ok();
            else
                return UnprocessableEntity(new BasicResponseDto() { StatusCode = 422, Description = "An error occurred while trying to create the job alert." });
        }

        [Authorize]
        [HttpPut]
        [Route("api/[controller]/alert")]
        public IActionResult EditJobPostingAlert([FromBody] JobPostingAlertDto jobPostingAlertDto)
        {
            if (jobPostingAlertDto == null)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Missing required fields." });
            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subsriberGuidClaim == null || subsriberGuidClaim == Guid.Empty)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "You must be logged in to edit a job alert." });
            else
                jobPostingAlertDto.Subscriber = new SubscriberDto() { SubscriberGuid = subsriberGuidClaim };

            bool isSuccess = JobPostingAlertFactory.SaveJobPostingAlert(_repositoryWrapper, _syslog, jobPostingAlertDto, _hangfireService);

            if (isSuccess)
                return Ok();
            else
                return UnprocessableEntity(new BasicResponseDto() { StatusCode = 422, Description = "An error occurred while trying to edit the job alert." });
        }

        [Authorize]
        [HttpDelete]
        [Route("api/[controller]/alert/{jobPostingAlertGuid}")]
        public IActionResult DeleteJobPostingAlert(Guid jobPostingAlertGuid)
        {
            if (jobPostingAlertGuid == null)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Missing required fields." });
            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var jobPostingAlerts = JobPostingAlertFactory.GetJobPostingAlertsBySubscriber(_repositoryWrapper, _syslog, subsriberGuidClaim);
            if (jobPostingAlerts == null)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Job alerts can only be deleted by the owner of the job alert." });
            var jobPostingAlertToDelete = jobPostingAlerts.Where(jpa => jpa.JobPostingAlertGuid == jobPostingAlertGuid).FirstOrDefault();
            if (jobPostingAlertToDelete == null)
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Job alerts can only be deleted by the owner of the job alert." });

            bool isSuccess = JobPostingAlertFactory.DeleteJobPostingAlert(_repositoryWrapper, _syslog, jobPostingAlertToDelete.JobPostingAlertGuid.Value, _hangfireService);

            if (isSuccess)
                return Ok();
            else
                return UnprocessableEntity(new BasicResponseDto() { StatusCode = 422, Description = "An error occurred while trying to delete the job alert." });
        }

        #endregion
    }
}