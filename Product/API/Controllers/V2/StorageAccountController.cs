using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.Models;
using System.Collections.Generic;
using UpDiddyApi.ApplicationCore.ActionFilter;
using UpDiddyApi.ApplicationCore.Services;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class StorageAccountController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30; 
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IServiceProvider _services;
        private readonly ICloudStorage _azureBlobStorage;
        private readonly ISubscriberService _subscriberService;
 


        #region constructor 

        public StorageAccountController(IServiceProvider services)
        {
            _services = services;
            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _subscriberService = _services.GetService<ISubscriberService>();
            _azureBlobStorage = _services.GetService<ICloudStorage>();

        }

        #endregion


        [HttpGet]
        [Authorize]
        [Route("intro-videos/sas/{subscriberGuid:guid}")]
        public async Task<IActionResult> GetSubscriberVideoUrlsForSubscriber(Guid subscriberGuid)
        {
            return Ok(await _subscriberService.GetVideoSASForSubscriber(subscriberGuid));
        }



        [HttpGet]
        [Authorize]
        [Route("intro-videos/sas")]
        public async Task<IActionResult> GetSubscriberVideoUrlsForContainer(Guid subscriberGuid)
        {
            return Ok(await _subscriberService.GetVideoSAS());
        }


    }
}