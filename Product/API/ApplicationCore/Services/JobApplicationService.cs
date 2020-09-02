using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Domain.Models;
using System.Threading;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger _syslog;
        private readonly UpDiddyDbContext _db = null;
        private ISubscriberService _subscriberService;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IHangfireService _hangfireService;
        private readonly ISysEmail _sysEmail;
        private readonly IResumeService _resumeService;

        public JobApplicationService(
            UpDiddyDbContext db,
            IRepositoryWrapper repositoryWrapper,
            ILogger<JobApplicationService> sysLog,
            IMapper mapper,
            ISubscriberService subscriberService,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IHangfireService hangfireService,
            ISysEmail sysEmail,
            IResumeService resumeService
            )
        {
            _repositoryWrapper = repositoryWrapper;
            _syslog = sysLog;
            _db = db;
            _mapper = mapper;
            _subscriberService = subscriberService;
            _configuration = configuration;
            _hangfireService = hangfireService;
            _sysEmail = sysEmail;
            _resumeService = resumeService;
        }

        public async Task<bool> IsSubscriberAppliedToJobPosting(int subscriberId, int jobPostingId)
        {
            IQueryable<JobApplication> jobPosting = _repositoryWrapper.JobApplication.GetAll();
            return await jobPosting.AnyAsync(x => x.SubscriberId == subscriberId && x.JobPostingId == jobPostingId);
        }

        public async Task<Guid> CreateJobApplication(Guid subscriberGuid, Guid jobGuid, ApplicationDto applicationDto)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication started at: {DateTime.UtcNow.ToLongDateString()}");
                JobPosting jobPosting = null;
                Subscriber subscriber = null;
                string ErrorMsg = string.Empty;
                int ErrorCode = 0;
                if (ValidateJobApplication(applicationDto, subscriberGuid, jobGuid, ref subscriber, ref jobPosting, ref ErrorMsg) == false)
                    throw new FailedValidationException(ErrorMsg);

                // create job application 
                JobApplication jobApplication = new JobApplication();
                BaseModelFactory.SetDefaultsForAddNew(jobApplication);
                jobApplication.JobApplicationGuid = Guid.NewGuid();
                jobApplication.JobPostingId = jobPosting.JobPostingId;
                jobApplication.SubscriberId = subscriber.SubscriberId;
                jobApplication.CoverLetter = applicationDto.CoverLetter == null ? string.Empty : applicationDto.CoverLetter;
                // Map Partner 
                if (string.IsNullOrEmpty(applicationDto.PartnerName) == false)
                {
                    PartnerType partnerType = await _repositoryWrapper.PartnerTypeRepository.GetPartnerTypeByName("ExternalSource");
                    // Get or create the referenced partner 
                    Partner partner = await _repositoryWrapper.PartnerRepository.GetOrCreatePartnerByName(applicationDto.PartnerName, partnerType);
                    jobApplication.PartnerId = partner.PartnerId;
                }

                _db.JobApplication.Add(jobApplication);
                _db.SaveChanges();

                // consolidated logic for job application emails
                await this.SendJobApplicationEmail(subscriber, jobPosting, applicationDto, jobApplication.JobApplicationGuid);
                SendJobConfirmationEmail(subscriber, jobPosting);

                return jobApplication.JobApplicationGuid;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication exception for subscriber {subscriberGuid.ToString()} and job {jobGuid}; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                throw new FailedValidationException("the job application could not be created");
            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication completed at: {DateTime.UtcNow.ToLongDateString()}");
            }
        }
        #region helper functions 

        private async Task SendJobApplicationEmail(Subscriber subscriber, JobPosting jobPosting, ApplicationDto applicationDto, Guid jobApplicationGuid)
        {
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationService.SendJobApplicationEmail started at: {DateTime.UtcNow.ToLongDateString()}");

                List<Attachment> attachments = new List<Attachment>();

                // attach a resume if a user has uploaded one (using the more recently created resume service)
                if (await _resumeService.HasSubscriberUploadedResume(subscriber.SubscriberGuid.Value))
                {
                    // use the resume service to create the attachment (as opposed to using the old methods for resume in SubscriberService)
                    var resume = await _resumeService.DownloadResume(subscriber.SubscriberGuid.Value);
                    Attachment resumeAttachment = new Attachment()
                    {
                        Content = resume.Base64EncodedData,
                        Filename = resume.FileName,
                        Type = resume.MimeType
                    };
                    attachments.Add(resumeAttachment);
                }

                Dictionary<string, bool> EmailAddressesToSend = new Dictionary<string, bool>();
                // configure an email for the recruiter (if one exists for the job posting)
                if (jobPosting.Recruiter != null)
                {
                    bool isExternalRecruiter = !jobPosting.Recruiter.SubscriberId.HasValue;
                    EmailAddressesToSend.Add(jobPosting.Recruiter.Email, isExternalRecruiter);
                }

                // configure separate emails for all VIPs (internal people that want to know when job applications occur)
                var vipEmails = _configuration.GetSection("SysEmail:VIPEmails").GetChildren();
                foreach (IConfigurationSection section in vipEmails)
                {
                    string vipEmail = section.Value;
                    EmailAddressesToSend.Add(vipEmail, false);
                }

                // send all templated emails
                foreach (string email in EmailAddressesToSend.Keys)
                {
                    Guid profileGuid = default(Guid);
                    try
                    {
                        bool isExternalMessage = EmailAddressesToSend[email];
                        string templateId = isExternalMessage ?
                            _configuration["SysEmail:Transactional:TemplateIds:JobApplication-Recruiter-External"].ToString() :
                            _configuration["SysEmail:Transactional:TemplateIds:JobApplication-Recruiter"].ToString();
                        profileGuid = await GetProfileGuid(subscriber.SubscriberGuid.Value);

                        object templateData = new
                        {
                            ApplicantName = applicationDto.FirstName + " " + applicationDto.LastName,
                            ApplicantFirstName = applicationDto.FirstName,
                            ApplicantLastName = applicationDto.LastName,
                            ApplicantEmail = subscriber.Email,
                            JobTitle = jobPosting.Title,
                            ApplicantUrl = _configuration["CareerCircle:ViewTalentUrl"] + profileGuid.ToString(),
                            JobUrl = _configuration["CareerCircle:ViewJobPostingUrl"] + jobPosting.JobPostingGuid.ToString(),
                            Subject = isExternalMessage ? $"{jobPosting.Company.CompanyName} job applicant via CareerCircle" : "Applicant Alert",
                            RecruiterGuid = jobPosting.Recruiter.RecruiterGuid,
                            JobApplicationGuid = jobApplicationGuid
                        };
                        _hangfireService.Enqueue(() => _sysEmail.SendTemplatedEmailAsync(email, templateId, templateData, Constants.SendGridAccount.Transactional, null, attachments, null, null, null, null));

                    }
                    catch (Exception ex)
                    {
                        _syslog.Log(
                            LogLevel.Information,
                            ex,
                            "***** JobApplicationController.SendJobApplicationEmail exception sending recruiter email for {jobApplicationGuid} on {subscriberGuid}/{profileGuid}.",
                            jobApplicationGuid,
                            subscriber.SubscriberGuid, profileGuid);
                    }
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController.SendJobApplicationEmail exception for jobApplicationGuid {jobApplicationGuid.ToString()}; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
            finally
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationService.SendJobApplicationEmail started at: {DateTime.UtcNow.ToLongDateString()}");
            }
        }

        private void SendJobConfirmationEmail(Subscriber subscriber, JobPosting jobPosting)
        {
            var templateData = new
            {
                firstName = subscriber.FirstName,
                lastName = subscriber.LastName,
                jobTitle = jobPosting.Title,
                company = jobPosting.Company?.CompanyName ?? ""
            };

            _hangfireService.Enqueue(() => _sysEmail.SendTemplatedEmailAsync(
                subscriber.Email,
                _configuration["SysEmail:Transactional:TemplateIds:JobApplicationUserConfirmation"],
                templateData,
                Constants.SendGridAccount.Transactional,
                null, // subject
                null, // attachments
                null, // sendAt
                null, // unsubscribeGroupId
                null, // cc
                null)); // bcc
        }

        // per Foley the requirment for a cover letter has been removed
        private bool ValidateJobApplication(ApplicationDto applicationDto, Guid subscriberGuid, Guid jobGuid, ref Subscriber subscriber, ref JobPosting jobPosting, ref string ErrorMsg)
        {

            if (applicationDto == null)
            {
                ErrorMsg = "Job application is required.";
                return false;
            }

            subscriber = SubscriberFactory.GetSubscriberWithSubscriberFiles(_repositoryWrapper, subscriberGuid).Result;
            if (subscriber == null)
            {
                ErrorMsg = $"Subscriber {subscriberGuid} does not exist.";
                return false;
            }

            //validate that resume and cover letter are present
            if (subscriber.SubscriberFile.FirstOrDefault() == null)
            {
                ErrorMsg = "Subscriber has not supplied a resume.";
                return false;
            }

            jobPosting = JobPostingFactory.GetJobPostingByGuid(_repositoryWrapper, jobGuid).Result;

            if (jobPosting == null)
            {
                ErrorMsg = $"Job posting {jobGuid} does not exist.";
                return false;
            }

            if (jobPosting.JobStatus != (int)JobPostingStatus.Active)
            {
                ErrorMsg = $"Job {jobPosting.JobPostingGuid} is not active.";
                return false;
            }

            // verify user has not already applied 
            JobApplication jobApplication = GetJobApplication(subscriber.SubscriberId, jobPosting.JobPostingId);
            if (jobApplication != null)
            {
                ErrorMsg = "User has already applied.";
                return false;
            }

            return true;
        }



        private JobApplication GetJobApplication(int subscriberId, int jobPostingID)
        {
            return _repositoryWrapper.JobApplication.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingID && s.SubscriberId == subscriberId)
                .FirstOrDefault();
        }

        public async Task<bool> HasJobApplication(Guid subscriberGuid, Guid jobPostingGuid)
        {
            return await _repositoryWrapper.JobApplication.HasSubscriberAppliedToJobPosting(subscriberGuid, jobPostingGuid);
        }

        private async Task<Guid> GetProfileGuid(Guid subscriberGuid)
        {
            var profile = await _repositoryWrapper.ProfileRepository.GetAll()
                .Include(p => p.Subscriber)
                .FirstOrDefaultAsync(
                    p => p.IsDeleted == 0 &&
                    p.Subscriber.IsDeleted == 0 &&
                    p.Subscriber.SubscriberGuid == subscriberGuid);

            if (profile == null) { throw new NotFoundException($"Could not find a profile for subscriber \"{subscriberGuid}\""); }

            return profile.ProfileGuid;
        }


        #endregion
    }
}