using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models.B2B;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.B2B
{
    public class InterviewRequestService : IInterviewRequestService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISysEmail _emailService;
        private readonly IHangfireService _hangfireService;
        private readonly ILogger _logger;

        public InterviewRequestService(
            IConfiguration configuration,
            IRepositoryWrapper repositoryWrapper,
            ISysEmail emailService,
            IHangfireService hangfireService,
            ILogger<InterviewRequestService> logger)
        {
            _configuration = configuration;
            _repositoryWrapper = repositoryWrapper;
            _emailService = emailService;
            _hangfireService = hangfireService;
            _logger = logger;
        }

        public async Task<Guid> SubmitInterviewRequest(HiringManagerDto hiringManager, Guid profileGuid, bool nonBlocking = true)
        {
            if (hiringManager == null) { throw new NotFoundException("hiring manager not found."); }

            var profile = await _repositoryWrapper.ProfileRepository.GetByGuid(profileGuid);
            if (profile == null) { throw new NotFoundException("profile was not found"); }

            var hiringManagerEntity = await _repositoryWrapper.HiringManagerRepository
                .GetAllWithTracking()
                .Include(hm => hm.Company)
                .Include(hm => hm.Subscriber)
                .Where(hm => hm.IsDeleted == 0 && hm.HiringManagerGuid == hiringManager.HiringManagerGuid)
                .FirstOrDefaultAsync();

            try
            {
                if (nonBlocking)
                {
                    _logger.LogInformation($"{nameof(InterviewRequestService)}:{nameof(SubmitInterviewRequest)} : Background job starting for hiring manager {hiringManager.HiringManagerGuid}");
                    _hangfireService.Enqueue<InterviewRequestService>(s => s.ProcessEmailRequests(hiringManager, profile, hiringManagerEntity.Subscriber.Email));
                }
                else
                {
                    _logger.LogInformation($"{nameof(InterviewRequestService)}:{nameof(SubmitInterviewRequest)} : awaiting _AddHiringManager for hiring manager {hiringManager.HiringManagerGuid}");
                    await ProcessEmailRequests(hiringManager, profile, hiringManagerEntity.Subscriber.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method} : Error occured while attempting to send email.", nameof(SubmitInterviewRequest));
            }


            var interviewRequest = new InterviewRequest
            {
                InterviewRequestGuid = Guid.NewGuid(),
                CreateGuid = Guid.NewGuid(),
                HiringManager = hiringManagerEntity,
                Profile = profile,
                DateRequested = DateTime.UtcNow
            };

            await _repositoryWrapper.InterviewRequestRepository.Create(interviewRequest);
            await _repositoryWrapper.InterviewRequestRepository.SaveAsync();

            return interviewRequest.InterviewRequestGuid;
        }

        private async Task ProcessEmailRequests(HiringManagerDto hiringManager, Profile profile, string hiringManagerEmail)
        {
            var candidateEmailTemplate = _configuration["SysEmail:Transactional:TemplateIds:InterviewRequestCandidate"];
            var recruiterEmailTemplate = _configuration["SysEmail:Transactional:TemplateIds:InterviewRequestRecruiter"];
            var recruiterEmailAddress = _configuration["SysEmail:Transactional:AdminEmailAddress"];

            await SendEmailToCandidate(
                profile.Email,
                candidateEmailTemplate,
                profile.FirstName,
                hiringManager.CompanyName);

            await SendEmailToRecruiter(
                recruiterEmailAddress,
                recruiterEmailTemplate,
                hiringManager.FirstName,
                hiringManager.LastName,
                hiringManagerEmail,
                hiringManager.CompanyName,
                profile.FirstName,
                profile.LastName,
                profile.Email);
        }

        #region Email Stuff

        private async Task SendEmailToCandidate(string emailAddress, string templateId, string firstName, string companyName)
            => await _emailService.SendTemplatedEmailWithReplyToAsync(
                emailAddress,
                templateId,
                BuildCandidateEmailTemplate(firstName, companyName),
                Constants.SendGridAccount.Transactional);

        private async Task SendEmailToRecruiter(
            string emailAddress,
            string templateId,
            string hiringManagerFirstName,
            string hiringManagerLastName,
            string hiringManagerEmail,
            string companyName,
            string firstName,
            string lastName,
            string email)
            => await _emailService.SendTemplatedEmailWithReplyToAsync(
                emailAddress,
                templateId,
                BuildRecruiterEmailTemplate(
                    hiringManagerFirstName,
                    hiringManagerLastName,
                    hiringManagerEmail,
                    companyName,
                    firstName,
                    lastName,
                    email),
                Constants.SendGridAccount.Transactional);

        private static object BuildCandidateEmailTemplate(string firstName, string companyName) => new
        {
            firstName,
            companyName
        };

        private static object BuildRecruiterEmailTemplate(
            string hiringManagerFirstName,
            string hiringManagerLastName,
            string hiringManagerEmail,
            string companyName,
            string firstName,
            string lastName,
            string email) => new
            {
                hiringManagerFirstName,
                hiringManagerLastName,
                hiringManagerEmail,
                companyName,
                firstName,
                lastName,
                email
            };

        #endregion Email Stuff
    }
}
