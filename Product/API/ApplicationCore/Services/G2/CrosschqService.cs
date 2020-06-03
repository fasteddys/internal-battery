using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            if (crosschqWebhookDto == null)
                throw new FailedValidationException("crosschqWebhookDto cannot be null");

            try
            {

            }
            catch(Exception ex)
            {

                throw;
            }
        }
    }
}
