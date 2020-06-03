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

            var interviewRequest = new InterviewRequest
            {
                InterviewRequestGuid = Guid.NewGuid(),
                CreateGuid = Guid.NewGuid(),
                HiringManager = hiringManagerEntity,
                Profile = profile,
                DateRequested = DateTime.UtcNow,
                Successful = false,
                Details = "Sending messages to recruiters and candidates."
            };

            await _repositoryWrapper.InterviewRequestRepository.Create(interviewRequest);
            await _repositoryWrapper.InterviewRequestRepository.SaveAsync();

            if (nonBlocking)
            {
                _logger.LogInformation($"{nameof(InterviewRequestService)}:{nameof(SubmitInterviewRequest)} : Background job starting for hiring manager {hiringManager.HiringManagerGuid}");
                _hangfireService.Enqueue<InterviewRequestService>(s => s.ProcessEmailRequestsLater(hiringManager, profile, hiringManagerEntity.Subscriber.Email, interviewRequest.InterviewRequestGuid));
            }
            else
            {
                try
                {
                    _logger.LogInformation($"{nameof(InterviewRequestService)}:{nameof(SubmitInterviewRequest)} : awaiting _AddHiringManager for hiring manager {hiringManager.HiringManagerGuid}");
                    var (successRecruiter, successCandidate) = await ProcessEmailRequestsNow(hiringManager, profile, hiringManagerEntity.Subscriber.Email);

                    interviewRequest.Details = $"Recruiter email {(successRecruiter ? "was sent" : "was not sent")}; Candidate email {(successCandidate ? "was sent" : "was not sent")}; ";
                    interviewRequest.ModifyDate = DateTime.UtcNow;
                    interviewRequest.Successful = successRecruiter && successCandidate;
                    await _repositoryWrapper.InterviewRequestRepository.SaveAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(InterviewRequestService)}:{nameof(SubmitInterviewRequest)} : Error occured while attempting to send email.");

                    interviewRequest.Details = $"Error occured while attempting to send email during hangfire queue: {ex.Message}";
                    interviewRequest.ModifyDate = DateTime.UtcNow;
                    interviewRequest.Successful = false;
                    await _repositoryWrapper.InterviewRequestRepository.SaveAsync();

                    throw;
                }
            }

            return interviewRequest.InterviewRequestGuid;
        }

        public async Task ProcessEmailRequestsLater(HiringManagerDto hiringManager, Profile profile, string hiringManagerEmail, Guid interviewRequestId)
        {
            var interviewRequest = await _repositoryWrapper.InterviewRequestRepository
                .GetByGuid(interviewRequestId);

            if (interviewRequest == null)
            {
                throw new NotFoundException($"Interview request {interviewRequestId} not found in the database.");
            }

            try
            {
                var (successRecruiter, successCandidate) = await ProcessEmailRequestsNow(hiringManager, profile, hiringManagerEmail);

                interviewRequest.Details = $"Recruiter email {(successRecruiter ? "was sent" : "was not sent")}; Candidate email {(successCandidate ? "was sent" : "was not sent")}; ";
                interviewRequest.ModifyDate = DateTime.UtcNow;
                interviewRequest.Successful = successRecruiter && successCandidate;
                await _repositoryWrapper.InterviewRequestRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(InterviewRequestService)}:{nameof(SubmitInterviewRequest)} : Error occured while attempting to send email during hangfire queue.");

                interviewRequest.Details = $"Error occured while attempting to send email during hangfire queue: {ex.Message}";
                interviewRequest.ModifyDate = DateTime.UtcNow;
                interviewRequest.Successful = false;
                await _repositoryWrapper.InterviewRequestRepository.SaveAsync();

                throw;
            }
        }

        private async Task<(bool, bool)> ProcessEmailRequestsNow(HiringManagerDto hiringManager, Profile profile, string hiringManagerEmail)
        {
            var candidateEmailTemplate = _configuration["SysEmail:Transactional:TemplateIds:InterviewRequestCandidate"];
            var recruiterEmailTemplate = _configuration["SysEmail:Transactional:TemplateIds:InterviewRequestRecruiter"];
            var recruiterEmailAddress = _configuration["SysEmail:InterviewRequestEmails"];

            var successRecruiter = await SendEmailToRecruiter(
                recruiterEmailAddress,
                recruiterEmailTemplate,
                hiringManager.FirstName,
                hiringManager.LastName,
                hiringManagerEmail,
                hiringManager.CompanyName,
                profile.FirstName,
                profile.LastName,
                profile.Email,
                profile.ProfileGuid);

            var successCandidate = await SendEmailToCandidate(
                profile.Email,
                candidateEmailTemplate,
                profile.FirstName,
                hiringManager.CompanyName);

            return (successRecruiter, successCandidate);
        }

        #region Email Stuff

        private async Task<bool> SendEmailToCandidate(string emailAddress, string templateId, string firstName, string companyName)
            => await _emailService.SendTemplatedEmailWithReplyToAsync(
                emailAddress,
                templateId,
                BuildCandidateEmailTemplate(firstName, companyName),
                Constants.SendGridAccount.Transactional);

        private async Task<bool> SendEmailToRecruiter(
            string emailAddress,
            string templateId,
            string hiringManagerFirstName,
            string hiringManagerLastName,
            string hiringManagerEmail,
            string companyName,
            string firstName,
            string lastName,
            string email,
            Guid profileGuid)
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
                    email,
                    profileGuid),
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
            string email,
            Guid profileGuid) => new
            {
                hiringManagerFirstName,
                hiringManagerLastName,
                hiringManagerEmail,
                companyName,
                firstName,
                lastName,
                email,
                profileGuid
            };

        #endregion Email Stuff
    }
}
