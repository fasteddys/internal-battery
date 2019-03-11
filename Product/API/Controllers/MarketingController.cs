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
using Newtonsoft.Json;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using System.Net;
using Microsoft.SqlServer.Server;

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


        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign")]
        public IActionResult CreateCampaign([FromBody] CampaignCreateDto campaignCreateDto)    
        {

            CampaignCreateResponseDto rVal = new CampaignCreateResponseDto();
            // transact operation that are fatal failures 
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // create campaign 
                    Campaign campaign = CampaignFactory.CreateCampaign(campaignCreateDto.Name, campaignCreateDto.Terms,
                        campaignCreateDto.Description, campaignCreateDto.StartDate, campaignCreateDto.EndDate);
                    
                    _db.Campaign.Add(campaign);
                    // save changes to get the campaign id
                    _db.SaveChanges();

                    // Create campaign phase 
                    CampaignPhase campaignPhase = CampaignPhaseFactory.CreateCampaignPhase(campaignCreateDto.PhaseName, campaignCreateDto.PhaseDescription, campaign.CampaignId);                    
                    _db.CampaignPhase.Add(campaignPhase);
                                                          
                    // Create campaign course invariants 
                    foreach ( CampaignCourseVariantCreateDto ccvCreate in campaignCreateDto.CourseVariants )
                    {
                        CampaignCourseVariant ccv = CampaignCourseVariantFactory.CreateCampaignCourseVariant(campaign.CampaignId, ccvCreate.CourseVariantId, ccvCreate.MaxRebateEligibilityInDays,
                            ccvCreate.IsEligibleForRebate, ccvCreate.RebateTypeId, ccvCreate.RefundId);                                           
                        _db.CampaignCourseVariant.Add(ccv);
 
                    }
                    // save changes                     
                    rVal.LandingPageUrl = $"https://careercircle.com/Home/Campaign/{campaign.CampaignGuid}/[%contact_guid%]?campaignphase={WebUtility.UrlEncode(campaignCreateDto.PhaseName)}";
                    rVal.TrackingImageUrl= $"'https://api.careercircle.io/api/tracking/{campaign.CampaignGuid}/[%contact_guid%]/8653122B-74F1-4020-8812-04C355CE56E7?campaignphase={WebUtility.UrlEncode(campaignCreateDto.PhaseName)}";
 
                    _db.SaveChanges();
                    transaction.Commit();
                    return Ok(rVal);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string CreateJson = JsonConvert.SerializeObject(campaignCreateDto);
                    _syslog.Log(LogLevel.Error, "MarketingController.CreateCampaign:  Exception: {@Exception} CampaignCreateDto: {CreateJson}", ex,CreateJson);
                    return StatusCode(500);
                }
            }
        }


        // get information about specified campaign
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

        // get statstics about specified campaign
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

        #region Campaign Contacts


        // Create contacts in the email provider 
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/{CampaignGuid}/publish-contact/{ContactListname}")]
        public IActionResult CreateProviderContactList(Guid campaignGuid, string ContactListName)
        {
            var campaign = CampaignFactory.GetCampaignByGuid(_db, campaignGuid);
            if (campaign == null)
                return BadRequest();
            ContactListName = WebUtility.UrlDecode(ContactListName);

            IList<EmailContactDto> TheList = _db.CampaignContact
                .Include(cc => cc.Contact)
                .ThenInclude(c => c.Subscriber)
                .Where(c => c.IsDeleted == 0 && c.CampaignId == campaign.CampaignId)
                .Select(c => new EmailContactDto
                {
                    first_name = c.Contact.FirstName,
                    last_name = c.Contact.LastName,
                    contact_guid = c.Contact.ContactGuid.ToString(),
                    email = c.Contact.Email,
                    subscriber_guid = c.Contact.Subscriber != null ? c.Contact.Subscriber.SubscriberGuid.ToString() : string.Empty
                }                                                
                )                
                .ToList();

            string ResponseJson = string.Empty;
            SendGridInterface sendGridInterface = new SendGridInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            HttpResponseMessage Response = sendGridInterface.CreateListAndAddContacts(ContactListName, TheList, ref ResponseJson);


            return Ok(Response);
        }
    
        // add contacts to the specified campaign.  This routine will add new contacts to the specified campaign as well as undelete
        // any contacts that have been logically deleted from the speciried campaign.
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/{CampaignGuid}/contact")]
        public IActionResult AddContacts(Guid campaignGuid, [FromBody] IList<Guid> contacts)
        {
            var campaign = CampaignFactory.GetCampaignByGuid(_db,campaignGuid);
            if (campaign == null)
                return BadRequest();

            DataTable table = new DataTable();
            table.Columns.Add("Guid", typeof(Guid));
            foreach (Guid ContactGuid in contacts)
            {
                table.Rows.Add(ContactGuid);
            }
            var contactGuids = new SqlParameter("@ContactGuids", table);            
            contactGuids.SqlDbType = SqlDbType.Structured;
            contactGuids.TypeName = "dbo.GuidList";
            var campaignId = new SqlParameter("@CampaignId", campaign.CampaignId);

            var spParams = new object[] { campaignId, contactGuids };
            // call stored procedure 
            var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Insert_CampaignContacts] 
                    @CampaignId,
                    @ContactGuids"
                    ,spParams);

            return Ok(rowsAffected);
        }

        // remove contacts from the specified campaign via a logical delete        
        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign/{CampaignGuid}/contact")]
        public IActionResult RemoveContacts(Guid campaignGuid, [FromBody] IList<Guid> contacts)
        {
            var campaign = CampaignFactory.GetCampaignByGuid(_db, campaignGuid);
            if (campaign == null)
                return BadRequest();

            DataTable table = new DataTable();
            table.Columns.Add("Guid", typeof(Guid));
            foreach (Guid ContactGuid in contacts)
            {
                table.Rows.Add(ContactGuid);
            }
            var contactGuids = new SqlParameter("@ContactGuids", table);
            contactGuids.SqlDbType = SqlDbType.Structured;
            contactGuids.TypeName = "dbo.GuidList";
            var campaignId = new SqlParameter("@CampaignId", campaign.CampaignId);

            var spParams = new object[] { campaignId, contactGuids };
            // call stored procedure 
            var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Delete_CampaignContacts] 
                    @CampaignId,
                    @ContactGuids"
                    , spParams);

            return Ok(rowsAffected);
        }



        #endregion

        #region Campaign Provider (currently SendGrid) Contacts 

        // add contacts to send grid
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign-provider/contact")]
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

        #region Campaign Provider (currently SendGrid) Contact Lists 

        // create an email contact list in send grid
        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/campaign-provider/contact-list")]
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
        [Route("api/[controller]/campaign-provider/contact-list")]
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
        [Route("api/[controller]/campaign-provider/contact-list/{ListName}")]
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
        [Route("api/[controller]/campaign-provider/contact-list/{ListId}")]
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
        [Route("api/[controller]/campaign-provider/contact-list/{ListName}")]
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
 
 