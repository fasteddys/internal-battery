using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddyApi.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using System.IO;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Factory;
using System.Data.SqlClient;
using AutoMapper.QueryableExtensions;
using System.Data;
using System.Web;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class CampaignsController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;
        private IB2CGraph _graphClient;
        private IAuthorizationService _authorizationService;
        private ICloudStorage _cloudStorage;

        public CampaignsController(UpDiddyDbContext db,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<SubscriberController> sysLog,
            IDistributedCache distributedCache,
            IB2CGraph client,
            ICloudStorage cloudStorage,
            IAuthorizationService authorizationService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _graphClient = client;
            _cloudStorage = cloudStorage;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Campaign> campaigns = _db.Campaign
                .Where(s => s.IsDeleted == 0)
                .ToList();
            return Ok(campaigns);
        }

        [HttpGet("{CampaignGuid}")]
        public async Task<IActionResult> GetCampaign(Guid CampaignGuid)
        {
            Campaign campaign = _db.Campaign
                .Where(s => s.IsDeleted == 0 && s.CampaignGuid == CampaignGuid)
                .FirstOrDefault();
            return Ok(campaign);
        }

        [HttpGet("partner-contact/{tinyId}")]
        public async Task<IActionResult> GetCampaignPartnerContactAsync(string tinyId)
        {
            CampaignPartnerContactDto campaignPartnerContact = _db.CampaignPartnerContact.Where(cpc => cpc.TinyId == tinyId && cpc.IsDeleted == 0)
                   .Include(cpc => cpc.Campaign)
                   .Include(cpc => cpc.PartnerContact).ThenInclude(pc => pc.Contact)
                   .Select(cpc => new CampaignPartnerContactDto()
                   {
                       CampaignGuid = cpc.Campaign.CampaignGuid,
                       Email = cpc.PartnerContact.Contact.Email,
                       FirstName = cpc.PartnerContact.Metadata["FirstName"].ToString(),
                       LastName = cpc.PartnerContact.Metadata["LastName"].ToString(),
                       IsCampaignActive = cpc.Campaign.StartDate <= DateTime.UtcNow && (cpc.Campaign.EndDate == null || cpc.Campaign.EndDate > DateTime.UtcNow),
                       PartnerContactGuid = cpc.PartnerContact.PartnerContactGuid.Value,
                       TargetedViewName = cpc.Campaign.TargetedViewName
                   })
                   .FirstOrDefault();
            return Ok(campaignPartnerContact);
        }
    }
}
