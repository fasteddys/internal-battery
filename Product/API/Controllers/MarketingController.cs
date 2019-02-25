using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;

namespace UpDiddyApi.Controllers
{
    public class MarketingController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected internal ILogger _syslog = null;
        private readonly IHttpClientFactory _httpClientFactory = null;

        public MarketingController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ProfileController> sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _httpClientFactory = httpClientFactory;


        }

        #region Campaigns 

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

        #endregion


        #region Contacts

        // add contacts to send grid
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/contact")]
        public IActionResult AddContacts([FromBody] IList<EmailContactDto> contacts)
        {
            string ResponseJson = string.Empty;
            SendGridInterface sendGridInterface = new SendGridInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);    
            HttpResponseMessage Response = sendGridInterface.AddContacts(contacts, ref ResponseJson);

            if (Response.StatusCode == HttpStatusCode.Created)
                return Ok(ResponseJson);
            else
                return BadRequest(ResponseJson);            
        }
        #endregion


        #region Contact Lists 

        // create an email contact list in send grid
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/contact-list")]
        public IActionResult CreateCampaignEmailList([FromBody] SendGridListDto List)
        {
            string ResponseJson = string.Empty;
            SendGridInterface sendGridInterface = new SendGridInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            HttpResponseMessage Response = sendGridInterface.CreateContactList(List, ref ResponseJson);

            if (Response.StatusCode == HttpStatusCode.Created)
                return Ok(ResponseJson);
            else
                return BadRequest(ResponseJson);
        }

        // get all send grid contact lists 
        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/contact-list")]
        public IActionResult GetCampaignEmailLists()
        {
            string ResponseJson = string.Empty;
            SendGridInterface sendGridInterface = new SendGridInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            HttpResponseMessage Response = sendGridInterface.GetContactLists(ref ResponseJson);

            if (Response.StatusCode == HttpStatusCode.OK)
                return Ok(ResponseJson);
            else
                return BadRequest(ResponseJson);
        }

        // get a specific contact list by name 
        [HttpGet]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/contact-list/{ListName}")]
        public IActionResult GetCampaignEmailList(string ListName)
        {
            string ResponseJson = string.Empty;
            SendGridInterface sendGridInterface = new SendGridInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            HttpResponseMessage Response = sendGridInterface.GetContactList(ListName, ref ResponseJson);

            if (Response.StatusCode == HttpStatusCode.OK)
                return Ok(ResponseJson);
            else
                return BadRequest(ResponseJson);
        }



        // add contacts to the specified contact list
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/contact-list/{ListId}")]
        public IActionResult AddContactstoList(string ListId, [FromBody] IList<EmailContactDto> contacts)
        {
            string ResponseJson = string.Empty;
            SendGridInterface sendGridInterface = new SendGridInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            HttpResponseMessage Response = sendGridInterface.AddContactsToList(ListId, contacts, ref ResponseJson);

            if (Response.StatusCode == HttpStatusCode.Created)
                return Ok(ResponseJson);
            else
                return BadRequest(ResponseJson);
        }

        // Create new contact list and add contacts to it. will also work with an existing named list.
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/contact-list/{ListName}")]
        public IActionResult CreateListAndContacts(string ListName, [FromBody] IList<EmailContactDto> contacts)
        {
            string ResponseJson = string.Empty;
            SendGridInterface sendGridInterface = new SendGridInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            HttpResponseMessage Response = sendGridInterface.CreateListAndAddContacts(ListName, contacts, ref ResponseJson);

            if (Response.StatusCode == HttpStatusCode.Created)
                return Ok(ResponseJson);
            else
                return BadRequest(ResponseJson);   
        }
        #endregion
    }
}