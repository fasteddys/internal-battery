using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces;

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
            List<Campaign> campaigns = await _db.Campaign
                .Where(s => s.IsDeleted == 0)
                .ToListAsync();
            return Ok(campaigns);
        }

        [HttpGet("{CampaignGuid}")]
        public async Task<IActionResult> GetCampaign(Guid CampaignGuid)
        {
            Campaign campaign = await _db.Campaign
                .Where(s => s.IsDeleted == 0 && s.CampaignGuid == CampaignGuid)
                .FirstOrDefaultAsync();
            return Ok(campaign);
        }

        [HttpGet("partner-contact/{tinyId}")]
        public async Task<IActionResult> GetCampaignPartnerContactAsync(string tinyId)
        {
            var cpc = await _db.CampaignPartnerContact.Where(x => x.TinyId == tinyId && x.IsDeleted == 0)
                   .Include(x => x.Campaign)
                   .Include(x => x.PartnerContact).ThenInclude(y => y.Contact)
                   .FirstOrDefaultAsync();

            CampaignPartnerContactDto campaignPartnerContact = null;

            if (cpc != null && cpc.Campaign != null && cpc.PartnerContact != null)
            {
                campaignPartnerContact = new CampaignPartnerContactDto()
                {
                    CampaignGuid = cpc.Campaign.CampaignGuid,
                    Email = cpc.PartnerContact.Contact.Email,
                    FirstName = cpc.PartnerContact.Metadata["FirstName"] != null ? cpc.PartnerContact.Metadata["FirstName"].ToString() : null,
                    LastName = cpc.PartnerContact.Metadata["LastName"] != null ? cpc.PartnerContact.Metadata["LastName"].ToString() : null,
                    IsCampaignActive = cpc.Campaign.StartDate <= DateTime.UtcNow && (!cpc.Campaign.EndDate.HasValue || cpc.Campaign.EndDate > DateTime.UtcNow),
                    PartnerContactGuid = cpc.PartnerContact.PartnerContactGuid.HasValue ? cpc.PartnerContact.PartnerContactGuid.Value : Guid.Empty,
                    TargetedViewName = cpc.Campaign.TargetedViewName
                };

            }
            return Ok(campaignPartnerContact);
        }
    }
}
