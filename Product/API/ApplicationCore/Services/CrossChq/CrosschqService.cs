using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models.CrossChq;
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

        public CrosschqService(
            UpDiddyDbContext context,
            IProfileService profileService,
            IRecruiterService recruiterService,
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            IHangfireService hangfireService,
            ICrossChqWebClient webClient)
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

                var request = new ReferenceRequest // TODO:  rename to "ReferenceRequestDto" when I know there won't be a merge collision!
                {
                    Candidate = new ReferenceCandidate // TODO:  rename to "ReferenceCandidateDto"
                    {
                        FirstName = profile.FirstName,
                        LastName = profile.LastName,
                        Email = profile.Email,
                        MobilePhone = profile.PhoneNumber
                    },
                    JobRole = referenceRequest.JobRole,
                    RequestorEmailAddress = recruiter.Email,
                    UseSMS = false,
                    ConfigurationParameters = new ReferenceRequestConfigurationParameters
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
                    HiringManager = new ReferenceHiringManager
                    {
                        FirstName = recruiter.FirstName,
                        LastName = recruiter.LastName,
                        Email = recruiter.Email
                    }
                };

                var requestId = await _webClient.PostReferenceRequestAsync(request);

                _logger.LogInformation($"CrosschqService:CreateReferenceRequest  CrossChq request_id: {requestId} ");


                await _repository.CrosschqRepository.AddReferenceCheck(profileGuid, recruiter.RecruiterGuid, request, requestId);

                return requestId;
            }
            catch(Exception ex)
            {
                _logger.LogError($"CrosschqService:CreateReferenceRequest  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CrosschqService:CreateReferenceRequest  Done for profileGuid: {profileGuid} ");
        }

        public async Task<List<ReferenceStatusDto>> RetrieveReferenceStatus(Guid profileGuid, Guid subscriberGuid)
        {
            var profile = await _profileService
                .GetProfileForRecruiter(profileGuid, subscriberGuid);

            // TODO:  Retrieve from Database

            var response = new List<ReferenceStatusDto>
            {

            };

            return response;
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

        #endregion
    }
}
