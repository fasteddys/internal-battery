using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Http;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using Skill = UpDiddyApi.Models.Skill;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SendgridEventService : ISendGridEventService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private IHangfireService _hangfireService;
        private ISysEmail _sysEmail;
        private readonly IServiceProvider _services;
        private ILogger _logger { get; set; }
        public SendgridEventService(IServiceProvider services, IRepositoryWrapper repositoryWrapper, IMapper mapper, IHangfireService hangfireService, IConfiguration configuration, ILogger<SendgridEventService> logger)
        {
            _services = services;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = configuration;
            _hangfireService = hangfireService;
            _sysEmail = _services.GetService<ISysEmail>();
            _logger = logger;
      
        }


        public async Task<bool> AddSendGridEvent(SendGridEventDto sendGridEvent)
        {
            _logger.LogInformation($"SendGridEventService:AddSendGridEvent  Starting");

            SendGridEvent sge = _mapper.Map<SendGridEvent>(sendGridEvent);
            await _repositoryWrapper.SendGridEventRepository.Create(sge);
            await _repositoryWrapper.SaveAsync();
            _logger.LogInformation($"SendGridEventService:AddSendGridEvent  Complete");

            return true;
        }



        public async Task<bool> AddSendGridEvents(List<SendGridEventDto> sendGridEvents)
        {

            _logger.LogInformation($"SendGridEventService:AddSendGridEvents  Starting");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(sendGridEvents);
            _repositoryWrapper.StoredProcedureRepository.InsertSendGridEvents(json);
            _logger.LogInformation($"SendGridEventService:AddSendGridEvents  Complete");

            return true;
        }

        public async Task<bool> PurgeSendGridEvents(int lookbackDays)
        {
            _logger.LogInformation($"SendGridEventService:PurgeSendGridEvents  Starting lookbackDays = {lookbackDays}");
            var rval = await _repositoryWrapper.StoredProcedureRepository.PurgeSendGridEvents(lookbackDays);
            _logger.LogInformation($"SendGridEventService:PurgeSendGridEvents  Complete");
            return true;
        }




    }
}
