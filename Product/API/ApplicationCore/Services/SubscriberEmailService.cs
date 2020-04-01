using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberEmailService : ISubscriberEmailService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper; 
        private IHangfireService _hangfireService { get; set; } 
        private ISysEmail _sysEmail;
        private readonly IButterCMSService _butterCMSService;
        private readonly ZeroBounceApi _zeroBounceApi;
 

        public SubscriberEmailService(UpDiddyDbContext context,
           IConfiguration configuration,
           ICloudStorage cloudStorage,
           IRepositoryWrapper repository,
           ILogger<SubscriberService> logger,
           IMapper mapper, 
           IHangfireService hangfireService, 
           ISysEmail sysEmail
        )
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;        
            _hangfireService = hangfireService; 
            _sysEmail = sysEmail;
            _zeroBounceApi = new ZeroBounceApi(_configuration, _repository, _logger); 
        }


        public async Task<List<SubscriberEmailStatisticDto>> GetEmailStatistics(Guid subscriberGuid)
        { 
            var Subscriber = await _repository.SubscriberRepository.GetByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist");

            List<SubscriberEmailStatisticDto> rVal = await _repository.StoredProcedureRepository.GetSubscriberEmailStatistics(Subscriber.Email);
            return rVal;
        }

    }
}
