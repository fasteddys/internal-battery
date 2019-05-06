using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using System.Net;
using Microsoft.SqlServer.Server;
// Use alias to avoid collistions on classname such as "Company"
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using AutoMapper.Configuration;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using UpDiddyApi.ApplicationCore;
using Hangfire;
using UpDiddyApi.Workflow;
using UpDiddyApi.Helpers.Job;
using System.Security.Claims;
using UpDiddyLib.Helpers;
using System.Dynamic;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using SendGrid.Helpers.Mail;

namespace UpDiddyApi.Controllers
{
 
    public class JobApplicationController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30;
        private readonly CloudTalent _cloudTalent = null;
        private ISysEmail _sysEmail;
        private ISubscriberService _subscriberService;

        #region constructor 
        public JobApplicationController(
            UpDiddyDbContext db, 
            IMapper mapper, 
            Microsoft.Extensions.Configuration.IConfiguration configuration, 
            ILogger<ProfileController> sysLog, 
            IHttpClientFactory httpClientFactory, 
            ISysEmail sysEmail,
            ISubscriberService subscriberService)

        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;
            _postingTTL = int.Parse(configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            _sysEmail = sysEmail;
            _subscriberService = subscriberService;
        }
        #endregion

        /// <summary>
        /// Return all of the job applications for the given subscriber 
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("api/[controller]/applicant/{subscriberGuid}")]
        public IActionResult GetJobApplicationForSubscriber(Guid subscriberGuid)
        {
            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplicationForSubscriber started at: {DateTime.UtcNow.ToLongDateString()}");

            Subscriber subscriber= SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            if (subscriber == null)
                return NotFound(new { code = 404, message = $"Subscriber {subscriberGuid} does not exist" });


            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subscriberGuid != subsriberGuidClaim)
                return BadRequest(new { code = 401, message = $"Job applications can only be viewed by applicant" });


            List<JobApplication> applications = JobApplicationFactory.GetJobApplicationsForSubscriber(_db, subscriber.SubscriberId);
            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplicationForSubscriber completed at: {DateTime.UtcNow.ToLongDateString()}");

            List<JobApplicationApplicantViewDto> rVal = _mapper.Map<List<JobApplicationApplicantViewDto>>(applications);

            string jobPostingUrl = _configuration["CareerCircle:ViewJobPostingUrl"];
            foreach (JobApplicationApplicantViewDto av in rVal)
                av.JobPostingUrl =     JobPostingFactory.JobPostingUrl(_configuration, av.JobPosting.JobPostingGuid.Value);

            return Ok(rVal);
        }



        /// <summary>
        /// Return all of the job applications for the specified job.  
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]/job/{jobPostingGuid}")]
        public IActionResult GetJobApplicationForPosting(Guid jobPostingGuid)
        {

            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplicationForPosting started at: {DateTime.UtcNow.ToLongDateString()}");

            JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuid(_db, jobPostingGuid);
            if (jobPosting == null)
                return NotFound(new { code = 404, message = $"Job posting {jobPostingGuid} does not exist" });


            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (jobPosting.Subscriber.SubscriberGuid != subsriberGuidClaim )
                return BadRequest(new { code = 401, message = $"Job applications can only be viewed by posting owner" });
 
            List<JobApplication> applications = JobApplicationFactory.GetJobApplicationsForPosting(_db, jobPosting.JobPostingId);
            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplicationForPosting completed at: {DateTime.UtcNow.ToLongDateString()}");            
            List<JobApplicationRecruiterViewDto> rVal = _mapper.Map<List<JobApplicationRecruiterViewDto>>(applications);

            // Fill in the view job seeker url
            foreach (JobApplicationRecruiterViewDto rv in rVal)
                rv.JobSeekerUrl = SubscriberFactory.JobseekerUrl(_configuration, rv.Subscriber.SubscriberGuid.Value);
            
            return Ok(rVal);
        }


        /// <summary>
        /// Return a single specific application.  The application can only be returned to the owner of the referenced job
        /// posting or the jobseeker who owns the application.  If the caller is the jobseeker, scrub the recuiters information
        /// </summary>
        /// <param name="jobApplicationGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("api/[controller]/{jobApplicationGuid}")]
        public IActionResult GetJobApplication(Guid jobApplicationGuid)
        {
            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplication started at: {DateTime.UtcNow.ToLongDateString()}");
            JobApplication jobApplication = JobApplicationFactory.GetJobApplicationByGuid(_db, jobApplicationGuid);
            if (jobApplication == null)
                return NotFound(new { code = 404, message = $"Job application {jobApplicationGuid} does not exist" });

            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (jobApplication.JobPosting.Subscriber.SubscriberGuid != subsriberGuidClaim && jobApplication.Subscriber.SubscriberGuid != subsriberGuidClaim)
                return BadRequest(new { code = 401, message = $"Job application can only be deleted by jobseeker or posting owner" });

            // Hide the recruiters information 
            if (jobApplication.Subscriber.SubscriberGuid == subsriberGuidClaim)
                jobApplication.JobPosting.Subscriber = null;

            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplication completed at: {DateTime.UtcNow.ToLongDateString()}");
            return Ok(_mapper.Map<JobApplicationDto>(jobApplication));
        }



        /// <summary>
        /// Delete a job application.  Restricted to jobseeker or recruiter who owns posting
        /// </summary>
        /// <param name="jobApplicationGuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize]
        [Route("api/[controller]/{jobApplicationGuid}")]
        public IActionResult DeleteJobApplication(Guid jobApplicationGuid)
        {
            try
            {
 
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:DeleteJobApplication started at: {DateTime.UtcNow.ToLongDateString()}");
                JobApplication jobApplication = JobApplicationFactory.GetJobApplicationByGuid(_db, jobApplicationGuid);
                if ( jobApplication == null )
                    return NotFound(new { code = 404, message = $"Job application {jobApplicationGuid} does not exist" });
           
                if (jobApplication.JobPosting == null)
                    return NotFound(new { code = 404, message = $"Job posting for application {jobApplication.JobApplicationGuid} does not exist" });

                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobApplication.JobPosting.Subscriber.SubscriberGuid != subsriberGuidClaim  && jobApplication.Subscriber.SubscriberGuid != subsriberGuidClaim)
                    return BadRequest(new { code = 401, message = $"Job application can only be deleted by jobseeker or posting owner" });

                jobApplication.IsDeleted = 1;
                jobApplication.ModifyDate = DateTime.UtcNow;
                _db.SaveChanges();

                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:DeleteJobApplication completed at: {DateTime.UtcNow.ToLongDateString()}");
                return Ok();
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:DeleteJobApplication exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }




        /// <summary>
        /// Create a job application 
        /// </summary>
        /// <param name="jobApplicationDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/[controller]")]
        public async Task<IActionResult> CreateJobApplication([FromBody] JobApplicationDto jobApplicationDto)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication started at: {DateTime.UtcNow.ToLongDateString()}");
                JobPosting jobPosting = null;
                Subscriber subscriber = null;
                string ErrorMsg = string.Empty;
                int ErrorCode = 0;
                if (JobApplicationFactory.ValidateJobApplication(_db, jobApplicationDto, ref subscriber, ref jobPosting, ref ErrorCode, ref ErrorMsg) == false)
                {
                    return BadRequest(new BasicResponseDto() { StatusCode = ErrorCode, Description = ErrorMsg });
                }

                // create job application 
                JobApplication jobApplication = new JobApplication();
                BaseModelFactory.SetDefaultsForAddNew(jobApplication);
                jobApplication.JobApplicationGuid = Guid.NewGuid();
                jobApplication.JobPostingId = jobPosting.JobPostingId;
                jobApplication.SubscriberId = subscriber.SubscriberId;
                jobApplication.CoverLetter = jobApplicationDto.CoverLetter == null ? string.Empty : jobApplicationDto.CoverLetter;
                _db.JobApplication.Add(jobApplication);
                _db.SaveChanges();

                Stream SubscriberResumeAsStream = await _subscriberService.GetResumeAsync(subscriber);

                // ERASE ONCE OTHER LOGIC IS IMPLEMENTED
                bool IsExternalRecruiter = true;
                string RecruiterCompany = "TEKsystems";

                // Send recruiter email alerting them to application
                BackgroundJob.Enqueue(() => _sysEmail.SendTemplatedEmailAsync
                    (jobPosting.Subscriber.Email,
                    _configuration["SysEmail:TemplateIds:JobApplication-Recruiter" +
                        (IsExternalRecruiter == true ? "-External" : string.Empty)],
                    new
                    {
                        ApplicantName = subscriber.FirstName + " " + subscriber.LastName,
                        JobTitle = jobPosting.Title,
                        ApplicantUrl = SubscriberFactory.JobseekerUrl(_configuration, subscriber.SubscriberGuid.Value),
                        JobUrl = JobPostingFactory.JobPostingUrl(_configuration, jobPosting.JobPostingGuid)
                    },
                    (IsExternalRecruiter == true ? $"{RecruiterCompany} job posting via CareerCircle" : "Applicant Alert"),
                    new List<Attachment>
                    {
                        new Attachment
                        {
                            Content = Convert.ToBase64String(Utils.StreamToByteArray(SubscriberResumeAsStream)),
                            Filename = Path.GetFileName(subscriber.SubscriberFile.FirstOrDefault().BlobName)
                        },
                        new Attachment
                        {
                            Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jobApplicationDto.CoverLetter)),
                            Filename = "CoverLetter.docx"
                        }
                    }
                ));
             
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication completed at: {DateTime.UtcNow.ToLongDateString()}");
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = $"{jobPosting.JobPostingGuid}" });
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication exception : {ex.Message}");
                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = ex.Message });
            }
        }



    }
}