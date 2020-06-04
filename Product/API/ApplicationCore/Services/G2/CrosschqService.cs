using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.Views;
using System.Security.Policy;

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


        public CrosschqService(
            UpDiddyDbContext context,
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            IHangfireService hangfireService
            )
        {
            _db = context;
            _configuration = configuration;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _hangfireService = hangfireService;
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
                if(crosschqWebhookDto.Progress == 100 && String.IsNullOrWhiteSpace(crosschqWebhookDto.Report_Full_Pdf))
                {
                    //test using https://images.homedepot-static.com/catalog/pdfImages/8b/8b47a061-1184-44b4-bf51-21b7f7ccd618.pdf
                    fullReportPdfBase64 = GetFileBase64Stream(new Uri(crosschqWebhookDto.Report_Full_Pdf, UriKind.Absolute));
                }
                await _repository.CrosschqRepository.UpdateReferenceCheck(crosschqWebhookDto, fullReportPdfBase64);
            }
            catch(Exception ex)
            {
                _logger.LogError($"CrosschqService:UpdateReferenceChkStatus  Error: {ex.ToString()} ");
                throw;
            }
            _logger.LogInformation($"CrosschqService:UpdateReferenceChkStatus  Done for Crosschq request_id: {crosschqWebhookDto.Id} ");

        }

        #region private methods
        private string GetFileBase64Stream(Uri fileUrl)
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
                _logger.LogError($"CrosschqService:GetFileBase64Stream  Error: {ex.ToString()} ");
                return null;
            }

        }

        #endregion
    }
}
