using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using UpDiddyApi.ApplicationCore;
using System.Security.Claims;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

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
        private readonly ICloudTalentService _cloudTalentService;
        private ISysEmail _sysEmail;
        private ISubscriberService _subscriberService;
        private readonly IHangfireService _hangfireService;
        private readonly IRepositoryWrapper _repositoryWrapper;


        #region constructor 
        public JobApplicationController(
            UpDiddyDbContext db, 
            IMapper mapper, 
            Microsoft.Extensions.Configuration.IConfiguration configuration, 
            ILogger<ProfileController> sysLog, 
            IHttpClientFactory httpClientFactory, 
            ISysEmail sysEmail,
            ISubscriberService subscriberService,
            IHangfireService hangfireService,
            IRepositoryWrapper repositoryWrapper,
            ICloudTalentService cloundTalentService )

        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;
            _postingTTL = int.Parse(configuration["JobPosting:PostingTTLInDays"]);
            _repositoryWrapper = repositoryWrapper;
            _sysEmail = sysEmail;
            _subscriberService = subscriberService;
            _hangfireService = hangfireService;
            _cloudTalentService = cloundTalentService;
        }
        #endregion

        /// <summary>
        /// Return all of the job applications for the given subscriber 
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("api/[controller]/applicant/{subscriberGuid}")]
        public async Task<IActionResult> GetJobApplicationForSubscriber(Guid subscriberGuid)
        {
            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplicationForSubscriber started at: {DateTime.UtcNow.ToLongDateString()}");

            Subscriber subscriber= await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
            if (subscriber == null)
                return NotFound(new { code = 404, message = $"Subscriber {subscriberGuid} does not exist" });


            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subscriberGuid != subsriberGuidClaim)
                return BadRequest(new { code = 401, message = $"Job applications can only be viewed by applicant" });


            List<JobApplication> applications = await JobApplicationFactory.GetJobApplicationsForSubscriber(_repositoryWrapper, subscriber.SubscriberId);
            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplicationForSubscriber completed at: {DateTime.UtcNow.ToLongDateString()}");

            List<JobApplicationApplicantViewDto> rVal = _mapper.Map<List<JobApplicationApplicantViewDto>>(applications); 
            foreach (JobApplicationApplicantViewDto av in rVal)
                av.JobPostingUrl =     JobPostingFactory.JobPostingFullyQualifiedUrl(_configuration, av.JobPosting);

            return Ok(rVal);
        }



        /// <summary>
        /// Return all of the job applications for the specified job.  
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        [Route("api/[controller]/job/{jobPostingGuid}")]
        public async Task<IActionResult> GetJobApplicationForPosting(Guid jobPostingGuid)
        {

            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplicationForPosting started at: {DateTime.UtcNow.ToLongDateString()}");

            JobPosting jobPosting = await JobPostingFactory.GetJobPostingByGuid(_repositoryWrapper, jobPostingGuid);
            if (jobPosting == null)
                return NotFound(new { code = 404, message = $"Job posting {jobPostingGuid} does not exist" });


            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (jobPosting.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim )
                return BadRequest(new { code = 401, message = $"Job applications can only be viewed by posting owner" });
 
            List<JobApplication> applications = await JobApplicationFactory.GetJobApplicationsForPosting(_repositoryWrapper, jobPosting.JobPostingId);
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
        public async Task<IActionResult> GetJobApplication(Guid jobApplicationGuid)
        {
            _syslog.Log(LogLevel.Information, $"***** JobApplicationController:GetJobApplication started at: {DateTime.UtcNow.ToLongDateString()}");
            JobApplication jobApplication = await JobApplicationFactory.GetJobApplicationByGuid(_repositoryWrapper, jobApplicationGuid);
            if (jobApplication == null)
                return NotFound(new { code = 404, message = $"Job application {jobApplicationGuid} does not exist" });

            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (jobApplication.JobPosting.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim && jobApplication.Subscriber.SubscriberGuid != subsriberGuidClaim)
                return BadRequest(new { code = 401, message = $"Job application can only be deleted by jobseeker or posting owner" });

            // Hide the recruiters information 
            if (jobApplication.Subscriber.SubscriberGuid == subsriberGuidClaim)
                jobApplication.JobPosting.Recruiter.Subscriber = null;

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
        public async Task<IActionResult> DeleteJobApplication(Guid jobApplicationGuid)
        {
            try
            {
 
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:DeleteJobApplication started at: {DateTime.UtcNow.ToLongDateString()}");
                JobApplication jobApplication = await JobApplicationFactory.GetJobApplicationByGuid(_repositoryWrapper, jobApplicationGuid);
                if ( jobApplication == null )
                    return NotFound(new { code = 404, message = $"Job application {jobApplicationGuid} does not exist" });
           
                if (jobApplication.JobPosting == null)
                    return NotFound(new { code = 404, message = $"Job posting for application {jobApplication.JobApplicationGuid} does not exist" });

                Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (jobApplication.JobPosting.Recruiter.Subscriber.SubscriberGuid != subsriberGuidClaim  && jobApplication.Subscriber.SubscriberGuid != subsriberGuidClaim)
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
                if (JobApplicationFactory.ValidateJobApplication(_repositoryWrapper, jobApplicationDto, ref subscriber, ref jobPosting, ref ErrorCode, ref ErrorMsg) == false)
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
                // Map Partner 
                if ( jobApplicationDto.Partner != null )
                {
                    PartnerType partnerType = await _repositoryWrapper.PartnerTypeRepository.GetPartnerTypeByName("ExternalSource");
                    // Get or create the referenced partner 
                    Partner partner = await _repositoryWrapper.PartnerRepository.GetOrCreatePartnerByName(jobApplicationDto.Partner.Name, partnerType);                    
                    jobApplication.PartnerId = partner.PartnerId;                  
                }
                    
                _db.JobApplication.Add(jobApplication);
                _db.SaveChanges();

                Stream SubscriberResumeAsStream = await _subscriberService.GetResumeAsync(subscriber);
                SubscriberResumeAsStream.Seek(0, SeekOrigin.Begin);
                string resumeEncoded = Convert.ToBase64String(Utils.StreamToByteArray(SubscriberResumeAsStream));

                bool IsExternalRecruiter = jobPosting.Recruiter.Subscriber == null;

                string RecruiterEmailToUse = jobPosting.Recruiter.Subscriber?.Email ?? jobPosting.Recruiter.Email;
                // Create a jobposting dto needed for the fully qualified job posting url in the recuriter email
                JobPostingDto jobPostingDto = _mapper.Map<JobPostingDto>(jobPosting);
                // Send recruiter email alerting them to application

                Dictionary<string, bool> EmailAddressesToSend = new Dictionary<string, bool>();
                EmailAddressesToSend.Add(RecruiterEmailToUse, IsExternalRecruiter);
                var VipEmails = _configuration.GetSection("SysEmail:VIPEmails").GetChildren();

                // Ensure all VIPs get the internal email template
                foreach (IConfigurationSection Child in VipEmails)
                {
                    string VipEmail = Child.Value;
                    EmailAddressesToSend.Add(VipEmail, false);
                }

                foreach (string Email in EmailAddressesToSend.Keys)
                {
                    _hangfireService.Enqueue(() => _sysEmail.SendTemplatedEmailAsync
                    (

                        _syslog,
                        Email,
                        _configuration["SysEmail:Transactional:TemplateIds:JobApplication-Recruiter" +
                            (EmailAddressesToSend[Email] == true ? "-External" : string.Empty)],
                        new
                        {
                            ApplicantName = jobApplicationDto.Subscriber.FirstName + " " + jobApplicationDto.Subscriber.LastName,
                            ApplicantFirstName = jobApplicationDto.Subscriber.FirstName,
                            ApplicantLastName = jobApplicationDto.Subscriber.LastName,
                            ApplicantEmail = subscriber.Email,
                            JobTitle = jobPosting.Title,
                            ApplicantUrl = SubscriberFactory.JobseekerUrl(_configuration, subscriber.SubscriberGuid.Value),
                            JobUrl = JobPostingFactory.JobPostingFullyQualifiedUrl(_configuration, jobPostingDto),
                            Subject = (IsExternalRecruiter == true ? $"{jobPosting.Company.CompanyName} job posting via CareerCircle" : "Applicant Alert"),
                            RecruiterGuid = jobPosting.Recruiter.RecruiterGuid,
                            JobApplicationGuid = jobApplication.JobApplicationGuid
                        },
                        Constants.SendGridAccount.Transactional,
                        null,
                        new List<Attachment>
                        {
                            new Attachment
                            {
                                Content = resumeEncoded,
                                Filename = Path.GetFileName(subscriber.SubscriberFile.FirstOrDefault().BlobName),
                                Type=subscriber.SubscriberFile.FirstOrDefault().MimeType
                            },
                            new Attachment
                            {
                                Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jobApplicationDto.CoverLetter)),
                                Filename = "CoverLetter.txt"
                            }
                        },
                        null,
                        null
                    ));
                }
                

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