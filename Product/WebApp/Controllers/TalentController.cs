using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
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

            CreateJobPostingViewModel model = await CreateJobPostingViewModel(jobPostingGuid);            
            return View("CreateJobPosting",model);
        }





        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        [HttpDelete]
        [Route("[controller]/jobPosting/{jobPostingGuid}/delete")]   
        public async Task<IActionResult> DeleteJobPosting(Guid jobPostingGuid)
        {
            try
            {
                await _api.DeleteJobPosting(jobPostingGuid);
                return Ok();
            }
            catch ( ApiException ex )
            {
                return BadRequest(new JsonResult(ex.ResponseDto )) ;
              
            }

        }


        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        [HttpPost]
        [Route("[controller]/jobPosting/{jobPostingGuid}/copy")]
        public async Task<IActionResult> CopyJobPosting(Guid jobPostingGuid)
        {

            await _api.CopyJobPosting(jobPostingGuid);
            return Ok();
        }


        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [Authorize]
        [HttpGet]
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
        public async Task<IActionResult> CreateJobPosting()
        {           
            CreateJobPostingViewModel model = await CreateJobPostingViewModel();
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
                    JobStatus = model.IsDraft == true ? (int)JobPostingStatus.Draft : (int)JobPostingStatus.Active,
                    IsPrivate = model.IsPrivate == true ? 1 : 0,
                    City = model.City,
                    PostingExpirationDateUTC = model.PostingExpirationDate,
                    Recruiter = new RecruiterDto()
                    {
                        Subscriber = new SubscriberDto()
                        {
                            SubscriberGuid = this.subscriber.SubscriberGuid
                        },

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
                    job.JobPostingSkills = new List<SkillDto>();
                    foreach ( string guid in skillGuids )
                    {
                        SkillDto skill = new SkillDto() { SkillGuid = Guid.Parse(guid) };
                        job.JobPostingSkills.Add(skill);
                    }
                }

                try
                {
                    if ( model.IsEdit )
                    {
                        job.JobPostingGuid = model.EditGuid;         
                        rVal = await _api.UpdateJobPostingAsync(job);                        
                    }                    
                    else                                           
                      rVal = await _api.AddJobPostingAsync(job);
                      
                }
                catch (ApiException ex)
                {
                    return Redirect(model.RequestPath + "?ErrorMsg=" + WebUtility.UrlEncode(ex.ResponseDto.Description));
                }
  
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



        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpGet]
        [Route("[controller]/subscriberData")]
        public async Task<IList<SubscriberDto>> SubscriberData(string searchFilter, string searchQuery = "")
        {      
            IList<SubscriberDto> subscribers = await _api.SubscriberSearchAsync(searchFilter, searchQuery);
            return subscribers;
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
            string AssestBaseUrl = _configuration["CareerCircle:AssetBaseUrl"];
            string CacheBuster = "?" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
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
                SubscriberGuid = subscriber.SubscriberGuid.Value,
                AvatarUrl = string.IsNullOrEmpty(subscriber.AvatarUrl) ? _configuration["CareerCircle:DefaultAvatar"] : AssestBaseUrl + subscriber.AvatarUrl + CacheBuster
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
        private async Task<CreateJobPostingViewModel> CreateJobPostingViewModel(Guid? jobPostingGuid = null )
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
                RequestPath = Request.Path,
                States = states.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.Name,
                    Selected = jobPostingDto?.Province?.Trim().ToLower() != null && jobPostingDto?.Province?.Trim().ToLower() == s.Name.Trim().ToLower()
                }),

                Industries = industries.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.IndustryGuid.ToString(),
                    Selected = jobPostingDto?.Industry?.IndustryGuid != null && jobPostingDto?.Industry?.IndustryGuid == s.IndustryGuid
                }),
                JobCategories = jobCategories.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.JobCategoryGuid.ToString(),                   
                    Selected = jobPostingDto?.JobCategory?.JobCategoryGuid != null && jobPostingDto?.JobCategory?.JobCategoryGuid == s.JobCategoryGuid
                }),
                ExperienceLevels = experienceLevels.Select(s => new SelectListItem()
                {
                    Text = s.DisplayName,
                    Value = s.ExperienceLevelGuid.ToString(),
                    Selected = jobPostingDto?.ExperienceLevel?.ExperienceLevelGuid != null && jobPostingDto?.ExperienceLevel?.ExperienceLevelGuid == s.ExperienceLevelGuid
                }),
                EducationLevels = educationLevels.Select(s => new SelectListItem()
                {
                    Text = s.Level,
                    Value = s.EducationLevelGuid.ToString(),
                    Selected = jobPostingDto?.EducationLevel?.EducationLevelGuid != null && jobPostingDto?.EducationLevel?.EducationLevelGuid == s.EducationLevelGuid
                }),
                EmploymentTypes = employmentTypes.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.EmploymentTypeGuid.ToString(),
                    Selected = jobPostingDto?.EmploymentType?.EmploymentTypeGuid != null && jobPostingDto?.EmploymentType?.EmploymentTypeGuid == s.EmploymentTypeGuid
                }),
                CompensationTypes = compensationType.Select(s => new SelectListItem()
                {
                    Text = s.CompensationTypeName,
                    Value = s.CompensationTypeGuid.ToString(),
                    Selected = jobPostingDto?.CompensationType?.CompensationTypeGuid != null && jobPostingDto?.CompensationType?.CompensationTypeGuid == s.CompensationTypeGuid
                }),
                SecurityClearances = SecurityClearances.Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.SecurityClearanceGuid.ToString(),
                    Selected = jobPostingDto?.SecurityClearance?.SecurityClearanceGuid != null && jobPostingDto?.SecurityClearance?.SecurityClearanceGuid == s.SecurityClearanceGuid
                }),
                RecruiterCompanies = companies.Select(s => new SelectListItem()
                {
                    Text = s.Company.CompanyName,
                    Value = s.Company.CompanyGuid.ToString(),
                    Selected = jobPostingDto?.Company?.CompanyGuid != null && jobPostingDto?.Company?.CompanyGuid == s.Company.CompanyGuid
                }),

                PostingExpirationDate = jobPostingDto == null ? DateTime.Now.AddDays(PostingExpirationInDays) : jobPostingDto.PostingExpirationDateUTC,
                ApplicationDeadline =  jobPostingDto == null  ? DateTime.Now.AddDays(PostingExpirationInDays) : jobPostingDto.ApplicationDeadlineUTC
            };

            model.Title = jobPostingDto == null ? string.Empty : jobPostingDto.Title;
            model.Description = jobPostingDto == null ? string.Empty : jobPostingDto.Description;
            model.City =  jobPostingDto == null ? string.Empty : jobPostingDto.City;
            model.StreetAddress = jobPostingDto == null ? string.Empty : jobPostingDto.StreetAddress;
            model.PostalCode = jobPostingDto == null ? string.Empty : jobPostingDto.PostalCode; 
            model.IsDraft = jobPostingDto == null ? true : jobPostingDto.JobStatus == (int) JobPostingStatus.Draft;
            model.IsPrivate = jobPostingDto == null ? false : jobPostingDto.IsPrivate == 1 ? true : false;
            model.IsAgency = jobPostingDto == null ? true : jobPostingDto.IsAgencyJobPosting;

            if (jobPostingDto != null && jobPostingDto.Compensation != 0)
                model.Compensation = jobPostingDto.Compensation;
            if (jobPostingDto != null && jobPostingDto.TelecommutePercentage != 0)
                model.Telecommute = jobPostingDto.TelecommutePercentage;

            // Initialize skills             
            if ( jobPostingDto!= null && jobPostingDto.JobPostingSkills != null )           
                model.Skills = jobPostingDto.JobPostingSkills;

            if (jobPostingDto != null)
            {
                model.IsEdit = true;
                model.EditGuid = jobPostingDto.JobPostingGuid.Value;
            }
            else
                model.IsEdit = false;

            // finally check to see if there is an error msg that needs to be displayed
            model.ErrorMsg = Request.Query["ErrorMsg"];

            return model;
        }


        #endregion


    }
}
