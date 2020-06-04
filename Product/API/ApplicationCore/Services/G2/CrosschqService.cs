using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2.CrossChq;
using UpDiddyApi.Models.Views;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Domain.Models.G2.CrossChq;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.G2
{
    public class CrosschqService: ICrosschqService
    {
        private readonly UpDiddyDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;
        private readonly IHangfireService _hangfireService;
        private readonly ICrossChqWebClient _webClient;

        public CrosschqService(
            UpDiddyDbContext context,
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            IHangfireService hangfireService,
            ICrossChqWebClient webClient)
        {
            _db = context;
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
                if(crosschqWebhookDto.Progress == 100 && !String.IsNullOrWhiteSpace(crosschqWebhookDto.Report_Full_Pdf))
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
            catch(Exception ex)
            {
                _logger.LogError($"CrosschqService:UpdateReferenceChkStatus  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CrosschqService:UpdateReferenceChkStatus  Done for Crosschq request_id: {crosschqWebhookDto.Id} ");

        }

        public async Task<string> CreateReferenceRequest(
            ProfileDto profile,
            RecruiterInfoDto recruiter,
            CrossChqReferenceRequestDto referenceRequest)
        {
            var request = new ReferenceRequest
            {
            };

            var requestId = await _webClient.PostReferenceRequestAsync(request);

            // TODO:  Persist the request and the resultant ID #2469

            return requestId;
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
