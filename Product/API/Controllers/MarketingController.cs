using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Controllers
{



    public class MarketingController : ControllerBase
    {


        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected internal ILogger _syslog = null;

        public MarketingController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
        }


        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign-detail/{CampaignGuid}")]
        public IActionResult CampaignDetails(Guid CampaignGuid)
        {

            var campaignInfo = _db.CampaignDetail.FromSql("System_CampaignDetails '" + CampaignGuid + "'")
                .ProjectTo<CampaignDetailDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(campaignInfo);
        }


        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign-statistic")]
        public IActionResult CampaignStatistics()
        {     
            var campaignInfo = _db.CampaignStatistic.FromSql("System_CampaignStatistics")
                .ProjectTo<CampaignStatisticDto>(_mapper.ConfigurationProvider)
                .ToList();                               
            return Ok(campaignInfo);
        }
    }
}