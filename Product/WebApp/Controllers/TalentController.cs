using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using UpDiddy.Api;
using UpDiddy.Authentication;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    [Authorize(Policy = "IsRecruiterPolicy")]
    public class TalentController : BaseController
    {
        private IApi _api;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public TalentController(IApi api,
        IConfiguration configuration,
        IHostingEnvironment env)
         : base(api)
        {
            _api = api;
            _env = env;
            _configuration = configuration;
        }

        #region Job Postings 



        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        [HttpGet]
        [Route("[controller]/jobPosting/{jobPostingGuid}/edit")]
        //public ViewResult JobPosting()
        public async Task<IActionResult> EditJobPosting(Guid jobPostingGuid)
        {

            CreateJobPostingViewModel model = await CreateViewModel(jobPostingGuid);
            return View("CreateJobPosting",model);
        }



        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        [HttpGet]
        //public ViewResult JobPosting()
        public async Task<IActionResult> JobPostings()
        {

            JobPostingsViewModel model = new JobPostingsViewModel()
            {
                jobPostings = await _api.GetJobPostingsForSubscriber(this.subscriber.SubscriberGuid.Value)
            };
          
            return View(model.jobPostings);

        }



        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        [HttpGet]
        //public ViewResult JobPosting()
        public async Task<IActionResult> CreateJobPosting()
        {           
            CreateJobPostingViewModel model = await CreateViewModel();
            return View(model);
        }




        
        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateJobPosting(CreateJobPostingViewModel model )
        {
            BasicResponseDto rVal = null;
            if ( ModelState.IsValid )
            {
                // create job posting dto and initailize all required fields 
                JobPostingDto job = new JobPostingDto()
                {
                    IsAgencyJobPosting = model.IsAgency,
                    Company = new CompanyDto()
                    {
                        CompanyGuid = model.SelectedRecruiterCompany.Value
                    },
                    Title = model.Title,
                    Description = model.Description,
                    JobStatus = model.IsDraft == true ? (int) JobPostingStatus.Draft :  (int) JobPostingStatus.Active,
                   //Province = 
                    City = model.City,
                    PostingExpirationDateUTC = model.PostingExpirationDate,
                    Subscriber = new SubscriberDto()
                    {
                        SubscriberGuid = this.subscriber.SubscriberGuid
                    },
                    Province = model.SelectedState,
                    ThirdPartyApply = false                    
                };

                job.ApplicationDeadlineUTC = model.ApplicationDeadline;
                if (model.Telecommute != null)
                    job.TelecommutePercentage = model.Telecommute.Value;
                if (model.Compensation != null)
                    job.Compensation = model.Compensation.Value;
                if (string.IsNullOrEmpty(model.PostalCode) == false)
                    job.PostalCode = model.PostalCode.Trim();
                if (string.IsNullOrEmpty(model.StreetAddress) == false)
                    job.StreetAddress = model.StreetAddress.Trim();
                if (model.SelectedIndustry != null)
                    job.Industry = new IndustryDto() { IndustryGuid = model.SelectedIndustry.Value };
                if (model.SelectedJobCategory != null)
                    job.JobCategory = new JobCategoryDto() { JobCategoryGuid = model.SelectedJobCategory.Value };
                if (model.SelectedSecurityClearance != null)
                    job.SecurityClearance = new SecurityClearanceDto() { SecurityClearanceGuid = model.SelectedSecurityClearance.Value };
                if (model.SelectedEmploymentType != null)
                    job.EmploymentType = new EmploymentTypeDto() { EmploymentTypeGuid = model.SelectedEmploymentType.Value };
                if (model.SelectedCompensationType != null)
                    job.CompensationType = new CompensationTypeDto() { CompensationTypeGuid = model.SelectedCompensationType.Value };
                if (model.SelectedExperienceLevel != null)
                    job.ExperienceLevel = new ExperienceLevelDto() { ExperienceLevelGuid = model.SelectedExperienceLevel.Value };
                if (model.SelectedExperienceLevel != null)
                    job.ExperienceLevel = new ExperienceLevelDto() { ExperienceLevelGuid = model.SelectedExperienceLevel.Value };
                if (model.SelectedEducationLevel != null)
                    job.EducationLevel = new EducationLevelDto() { EducationLevelGuid = model.SelectedEducationLevel.Value };
                if ( string.IsNullOrEmpty(model.SelectedSkills) == false )
                {
                    string[] skillGuids = model.SelectedSkills.Trim().Split(',');
                    job.Skills = new List<SkillDto>();
                    foreach ( string guid in skillGuids )
                    {
                        SkillDto skill = new SkillDto() { SkillGuid = Guid.Parse(guid) };
                        job.Skills.Add(skill);
                    }
                }
                rVal =   await _api.AddJobPostingAsync(job);
                  
            }
 
            return RedirectToAction("JobPostings");

        }

        #endregion

        [Authorize]
        [HttpGet]
        public ViewResult Subscribers()
        {
            var subscriberSourcesDto = _api.SubscriberSourcesAsync().Result.OrderByDescending(ss => ss.Count);

            var selectListItems = subscriberSourcesDto.Select(ss => new SelectListItem()
            {
                Text = $"{ss.Name} ({ss.Count})",
                Value = ss.Referrer
            })
            .AsEnumerable();

            return View(new TalentSubscriberViewModel() { SubscriberSources = selectListItems });
        }

        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult> SubscriberGrid(string searchAndFilter)
        {
            string searchFilter;
            string searchQuery;

            if (searchAndFilter != null)
            {
                var jObject = JObject.Parse(searchAndFilter);
                searchFilter = jObject["searchFilter"].Value<string>();
                searchQuery = jObject["searchQuery"].Value<string>();
            }
            else
            {
                searchFilter = "any";
                searchQuery = string.Empty;
            }
            IList<SubscriberDto> subscribers = await _api.SubscriberSearchAsync(searchFilter, searchQuery);
            return PartialView("_SubscriberGrid", subscribers);
        }

        [Authorize]
        [HttpGet]
        [Route("/Talent/Subscriber/{subscriberGuid}")]
        public async Task<IActionResult> SubscriberAsync(Guid subscriberGuid)
        {
            SubscriberDto subscriber = await _api.SubscriberAsync(subscriberGuid, false);

            SubscriberViewModel subscriberViewModel = new SubscriberViewModel()
            {
                FirstName = subscriber.FirstName,
                LastName = subscriber.LastName,
                Email = subscriber.Email,
                PhoneNumber = subscriber.PhoneNumber,
                Address = subscriber.Address,
                City = subscriber.City,
                State = subscriber.State?.Code,
                Country = subscriber.State?.Country?.Code3,
                FacebookUrl = subscriber.FacebookUrl,
                GithubUrl = subscriber.GithubUrl,
                LinkedInUrl = subscriber.LinkedInUrl,
                StackOverflowUrl = subscriber.StackOverflowUrl,
                TwitterUrl = subscriber.TwitterUrl,
                WorkHistory = subscriber.WorkHistory,
                EducationHistory = subscriber.EducationHistory,
                Skills = subscriber.Skills,
                Enrollments = subscriber.Enrollments,
                ResumeFileGuid = subscriber.Files?.FirstOrDefault()?.SubscriberFileGuid,
                ResumeFileName = subscriber.Files?.FirstOrDefault()?.SimpleName,
                SubscriberGuid = subscriber.SubscriberGuid.Value
            };
            
            return View("Subscriber", subscriberViewModel);
        }
        
        [HttpGet]
        [Authorize]
        [Route("/Talent/Subscriber/{subscriberGuid}/File/{fileGuid}")]
        public async Task<IActionResult> DownloadFileAsync(Guid subscriberGuid, Guid fileGuid)
        {
            HttpResponseMessage response = await _api.DownloadFileAsync(subscriberGuid, fileGuid);
            Stream stream = await response.Content.ReadAsStreamAsync();
            return File(stream, "application/octet-stream",
                response.Content.Headers.ContentDisposition.FileName.Replace("\"", ""));
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpDelete]
        [Route("/Talent/Subscriber/{subscriberGuid}")]
        public async Task<IActionResult> DeleteSubscriberAsync(Guid subscriberGuid)
        {
            var isSubscriberDeleted = await _api.DeleteSubscriberAsync(subscriberGuid);
            return new JsonResult(isSubscriberDeleted);
        }

        #region private helper functions
        private async Task<CreateJobPostingViewModel> CreateViewModel(Guid? jobPostingGuid = null )
        {
            JobPostingDto jobPostingDto = null;
            if ( jobPostingGuid != null )
            {
                    jobPostingDto = await _api.GetJobPostingByGuid(jobPostingGuid.Value);
            }

            Guid USCountryGuid = Guid.Parse(_configuration["CareerCircle:USCountryGuid"]);

            var states = await _Api.GetStatesByCountryAsync(USCountryGuid);
            var industries = await _Api.GetIndustryAsync();
            var jobCategories = await _Api.GetJobCategoryAsync();
            var educationLevels = await _api.GetEducationLevelAsync();
            var experienceLevels = await _api.GetExperienceLevelAsync();
            var employmentTypes = await _api.GetEmploymentTypeAsync();
            var compensationType = await _api.GetCompensationTypeAsync();
            var SecurityClearances = await _api.GetSecurityClearanceAsync();
            var companies = await _api.GetRecruiterCompaniesAsync(this.subscriber.SubscriberGuid.Value);
            int PostingExpirationInDays = int.Parse(_configuration["CareerCircle:PostingExpirationInDays"]);
            CreateJobPostingViewModel model = new CreateJobPostingViewModel()
            {
                States = states.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.Name,
                    Selected = s.StateGuid == this.subscriber?.State?.StateGuid
                }),

                Industries = industries.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.IndustryGuid.ToString()
                }),
                JobCategories = jobCategories.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.JobCategoryGuid.ToString()
                }),
                ExperienceLevels = experienceLevels.Select(s => new SelectListItem()
                {
                    Text = s.DisplayName,
                    Value = s.ExperienceLevelGuid.ToString()
                }),
                EducationLevels = educationLevels.Select(s => new SelectListItem()
                {
                    Text = s.Level,
                    Value = s.EducationLevelGuid.ToString()
                }),
                EmploymentTypes = employmentTypes.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.EmploymentTypeGuid.ToString()
                }),
                CompensationTypes = compensationType.Select(s => new SelectListItem()
                {
                    Text = s.CompensationTypeName,
                    Value = s.CompensationTypeGuid.ToString()
                }),
                SecurityClearances = SecurityClearances.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.SecurityClearanceGuid.ToString()
                }),
                RecruiterCompanies = companies.Select(s => new SelectListItem()
                {
                    Text = s.Company.CompanyName,
                    Value = s.Company.CompanyGuid.ToString()
                }),

                PostingExpirationDate = DateTime.Now.AddDays(PostingExpirationInDays),
                ApplicationDeadline = DateTime.Now.AddDays(PostingExpirationInDays)
            };

            return model;

        }


        #endregion


    }
}
