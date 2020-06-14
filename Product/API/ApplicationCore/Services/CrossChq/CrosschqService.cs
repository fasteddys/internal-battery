using AutoMapper;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.CrossChq;
using UpDiddyLib.Domain.Models.CrossChq;
using UpDiddyLib.Helpers;
using Constants = UpDiddyLib.Helpers.Constants;
using IProfileService = UpDiddyApi.ApplicationCore.Interfaces.Business.G2.IProfileService;

namespace UpDiddyApi.ApplicationCore.Services.CrossChq
{
    public class CrosschqService : ICrosschqService
    {
        private readonly UpDiddyDbContext _db;
        private readonly IProfileService _profileService;
        private readonly IRecruiterService _recruiterService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;
        private readonly IHangfireService _hangfireService;
        private readonly ICrossChqWebClient _webClient;
        private readonly ISysEmail _emailService;
        private readonly CrossChqOptions _options;

        public CrosschqService(
            UpDiddyDbContext context,
            IProfileService profileService,
            IRecruiterService recruiterService,
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            IHangfireService hangfireService,
            ICrossChqWebClient webClient,
            ISysEmail emailService,
            IOptions<CrossChqOptions> optionsAccessor)
        {
            _db = context;
            _profileService = profileService;
            _recruiterService = recruiterService;
            _configuration = configuration;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _hangfireService = hangfireService;
            _webClient = webClient;
            _emailService = emailService;
            _options = optionsAccessor.Value;
        }

        public async Task UpdateReferenceChkStatus(CrosschqWebhookDto crosschqWebhookDto)
        {
            _logger.LogInformation($"CrosschqService:UpdateReferenceChkStatus  Starting for Crosschq request_id {crosschqWebhookDto.Id} ");
            if (crosschqWebhookDto == null)
                throw new FailedValidationException("crosschqWebhookDto cannot be null");
            if (string.IsNullOrWhiteSpace(crosschqWebhookDto.Id))
                throw new FailedValidationException("crosschqWebhookDto.Id cannot be null");

            try
            {
                string fullReportPdfBase64 = null;
                if (crosschqWebhookDto.Progress == 100 && !String.IsNullOrWhiteSpace(crosschqWebhookDto.Report_Full_Pdf))
                {
                    fullReportPdfBase64 = GetFileBase64String(new Uri(crosschqWebhookDto.Report_Full_Pdf, UriKind.Absolute));
                }
                string summaryReportPdfBase64 = null;
                if (crosschqWebhookDto.Progress == 100 && !String.IsNullOrWhiteSpace(crosschqWebhookDto.Report_Summary_Pdf))
                {
                    summaryReportPdfBase64 = GetFileBase64String(new Uri(crosschqWebhookDto.Report_Summary_Pdf, UriKind.Absolute));
                }
                await _repository.CrosschqRepository.UpdateReferenceCheck(crosschqWebhookDto, fullReportPdfBase64, summaryReportPdfBase64);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CrosschqService:UpdateReferenceChkStatus  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CrosschqService:UpdateReferenceChkStatus  Done for Crosschq request_id: {crosschqWebhookDto.Id} ");

        }

        public async Task<string> CreateReferenceRequest(
            Guid profileGuid,
            Guid subscriberGuid,
            CrossChqReferenceRequestDto referenceRequest)
        {
            try
            {
                var profile = await _profileService
                    .GetProfileForRecruiter(profileGuid, subscriberGuid);

                var recruiter = await _recruiterService
                    .GetRecruiterBySubscriberAsync(subscriberGuid);

                var request = new ReferenceRequestDto
                {
                    Candidate = new ReferenceCandidateDto
                    {
                        FirstName = profile.FirstName,
                        LastName = profile.LastName,
                        Email = profile.Email,
                        MobilePhone = PreparePhoneNumber(profile.PhoneNumber)
                    },
                    JobRole = referenceRequest.JobRole,
                    RequestorEmailAddress = recruiter.Email,
                    UseSMS = false,
                    ConfigurationParameters = new ConfigurationParametersDto
                    {
                        Managers = referenceRequest?.ConfigurationParameters.Managers ?? 0,
                        Employees = referenceRequest?.ConfigurationParameters.Employees ?? 0,
                        Peers = referenceRequest?.ConfigurationParameters.Peers ?? 0,
                        Business = referenceRequest?.ConfigurationParameters.Business ?? 0,
                        Social = referenceRequest?.ConfigurationParameters.Social ?? 0
                    },
                    SendPastDueNotification = false,
                    SendCompletedNotification = false,
                    CandidateMessage = referenceRequest.CandidateMessage,
                    JobPosition = referenceRequest.JobPosition,
                    HiringManager = new ReferenceHiringManagerDto
                    {
                        FirstName = recruiter.FirstName,
                        LastName = recruiter.LastName,
                        Email = recruiter.Email
                    }
                };

                var requestId = await _webClient.PostReferenceRequestAsync(request);

                _logger.LogInformation($"CrosschqService:CreateReferenceRequest  CrossChq request_id: {requestId} ");

                await _repository.CrosschqRepository.AddReferenceCheck(profileGuid, recruiter.RecruiterGuid, request, requestId);

                await SendSuccessEmail(profile.Email);

                return requestId;
            }
            catch (AlreadyExistsException aee)
            {
                _logger.LogError(aee, "Error occurred while retrieving a Reference Request");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CrosschqService:CreateReferenceRequest  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CrosschqService:CreateReferenceRequest  Done for profileGuid: {profileGuid} ");
        }

        public async Task<List<ReferenceStatusDto>> RetrieveReferenceStatus(Guid profileGuid)
        {
            try
            {
                _logger.LogInformation("CrosschqService:RetrieveReferenceStatus: Fetching reference status for {profileGuid}", profileGuid);

                var referenceCheck = await _repository.CrosschqRepository
                    .GetReferenceCheckByProfileGuid(profileGuid);

                return referenceCheck
                    .Select(rc => (
                        referenceCheck: rc,
                        status: rc.ReferenceCheckStatus
                            .OrderByDescending(s => s.CreateDate)
                            .FirstOrDefault()))
                    .Select((rc, status) => new ReferenceStatusDto
                    {
                        ReferenceCheckId = rc.referenceCheck.ReferenceCheckGuid,
                        Status = rc.status?.Status ?? "",
                        JobRole = rc.referenceCheck.ReferenceCheckType,
                        JobPosition = rc.referenceCheck.CandidateJobTitle,
                        PercentComplete = rc.status?.Progress ?? 0,
                        CreateDate = rc.status?.CreateDate,
                        References = rc.referenceCheck.CandidateReference
                            ?.Select(r => new ReferenceDto
                            {
                                FirstName = r.FirstName,
                                LastName = r.LastName,
                                Email = r.Email,
                                MobilePhone = r.PhoneNumber,
                                Status = r.Status
                            })
                            .ToList()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CrosschqService:RetrieveReferenceStatus: Error fetching reference status: {errorMsg}", ex.ToString());
                throw;
            }
        }

        public async Task<ReferenceCheckReportDto> GetReferenceCheckReportPdf(Guid referenceCheckGuid, string reportType)
        {
            _logger.LogInformation($"CrosschqService:GetReferenceCheckReportPdf  Starting for referenceCheckGuid {referenceCheckGuid} ");
            if (referenceCheckGuid == Guid.Empty)
                throw new FailedValidationException("referenceCheckGuid cannot be null or empty");
            if (string.IsNullOrWhiteSpace(reportType))
                throw new FailedValidationException("reportType cannot be null or empty. Allow values: Full/Summary.");

            try
            {
                var referenceCheckReportEntity = await _repository.CrosschqRepository.GetReferenceCheckReportPdf(referenceCheckGuid, reportType);

                var referenceCheckReportDto = _mapper.Map<ReferenceCheckReportDto>(referenceCheckReportEntity);
                return referenceCheckReportDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CrosschqService:GetReferenceCheckReportPdf  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CrosschqService:GetReferenceCheckReportPdf  Done for referenceCheckGuid: {referenceCheckGuid} ");

        }

        #region private methods
        private string GetFileBase64String(Uri fileUrl)
        {
            if (fileUrl == null) return null;

            string fileBase64 = null;
            try
            {
                using (WebClient client = new WebClient())
                {
                    var bytes = client.DownloadData(fileUrl);
                    fileBase64 = Convert.ToBase64String(bytes);
                }

                return fileBase64;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CrosschqService:GetFileBase64String  Error: {ex.ToString()} ");
                return null;
            }
        }

        private async Task SendSuccessEmail(string emailAddress)
            => await _emailService.SendTemplatedEmailAsync(
                emailAddress,
                _options.NotificationEmailTemplate,
                null,
                Constants.SendGridAccount.Transactional);

        private static string PreparePhoneNumber(string phoneNumber)
        {
            var gasOnaFire = new BadRequestException("Invalid or missing phone number");

            if(string.IsNullOrEmpty(phoneNumber)) { throw gasOnaFire; }

            var digits = new Regex(@"\d")
                .Matches(phoneNumber)
                .Select(regex => regex.Value)
                .ToList();

            if (digits.Count < 10) { throw gasOnaFire; }

            return $"+1{string.Join(null, digits)}";
        }

        #endregion
    }
}
