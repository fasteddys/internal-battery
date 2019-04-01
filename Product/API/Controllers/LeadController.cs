using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using UpDiddyLib.Dto.Marketing;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class LeadController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;
        private IB2CGraph _graphClient;
        private IAuthorizationService _authorizationService;
        private ICloudStorage _cloudStorage;

        public LeadController(UpDiddyDbContext db,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<LeadController> sysLog,
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

        /*
         * Require a bearer token that only the azure api endpoint knows (secret key)
         * Require a partner key which identifies who is sending the lead data 
         * Validate the message sent, store lead data, send a response to the caller
         */

        [HttpGet]
        public async Task<IActionResult> GetLeadAsync(Guid leadIdentifier)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> InsertLeadAsync([FromBody] LeadRequestDto leadRequest)
        {
            // lookup Partner by the ApiToken provided
            var partner = _db.Partner
                .Include(p => p.PartnerType)
                .Where(p => p.ApiToken == leadRequest.ApiToken && p.PartnerType.Name == "Pay Per Lead")
                .FirstOrDefault();

            // abort immediately if we do not recognize the Partner
            if (partner == null)
                return StatusCode((int)HttpStatusCode.Unauthorized, "Invalid ApiToken provided");

            // by default a lead is accepted
            bool isBillable = true;

            // lead statuses which will be saved for the lead go here
            List<PartnerContactLeadStatus> partnerContactLeadStatuses = new List<PartnerContactLeadStatus>();

            // perform simplistic dupe check (name or email, infinite dupe window)            
            var email = new SqlParameter("@Email", leadRequest.EmailAddress);
            var phone = new SqlParameter("@Phone", leadRequest.MobilePhone);
            var isDupe = new SqlParameter { ParameterName = "@IsDupe", SqlDbType = SqlDbType.Bit, Size = 1, Direction = ParameterDirection.Output };

            var spParams = new object[] { email, phone, isDupe };
            var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Get_LeadDupeCheck] 
                    @Email,
                    @Phone,
	                @IsDupe OUTPUT", spParams);

            if (Convert.ToBoolean(isDupe.Value))
            {
                partnerContactLeadStatuses.Add(new PartnerContactLeadStatus()
                {
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = 0,
                    CreateGuid = Guid.Empty,
                    LeadStatusId = 3,
                    PartnerContactLeadStatusGuid = Guid.NewGuid()
                });

                isBillable = false;
            }

            // check required fields
            if (string.IsNullOrWhiteSpace(leadRequest.EmailAddress) || string.IsNullOrWhiteSpace(leadRequest.FirstName) || string.IsNullOrWhiteSpace(leadRequest.LastName) || string.IsNullOrWhiteSpace(leadRequest.MobilePhone))
            {
                partnerContactLeadStatuses.Add(new PartnerContactLeadStatus()
                {
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = 0,
                    CreateGuid = Guid.Empty,
                    LeadStatusId = 5,
                    PartnerContactLeadStatusGuid = Guid.NewGuid()
                });

                isBillable = false;
            }

            // check test flag
            if (leadRequest.IsTest.HasValue && leadRequest.IsTest.Value)
            {
                partnerContactLeadStatuses.Add(new PartnerContactLeadStatus()
                {
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = 0,
                    CreateGuid = Guid.Empty,
                    LeadStatusId = 4,
                    PartnerContactLeadStatusGuid = Guid.NewGuid()
                });

                isBillable = false;
            }

            // store Contact, PartnerContact, LeadStatuses, IsBillable flag - need to decide where to set each
            // basic properties in PartnerContact.MetaDataJSON (fname, lname, phone, email, address1, address2, city, state, postal, country
            // put external lead id in sourcesystemidentifier
            // lead statuses in xref table
            // lead identifier we give to vendor can be PartnerContactGuid
            // isbillable - where does this belong? can add to partnercontact for now?
            try
            {
                _db.PartnerContact.Add(new PartnerContact()
                {
                    Contact = new Contact()
                    {
                        ContactGuid = Guid.NewGuid(),
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        Email = leadRequest.EmailAddress,
                        IsDeleted = 0
                    },
                    IsBillable = isBillable,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0,
                    Metadata = JObject.Parse(JsonConvert.SerializeObject(leadRequest)),
                    PartnerContactGuid = Guid.NewGuid(),
                    PartnerId = partner.PartnerId,
                    SourceSystemIdentifier = leadRequest.ExternalLeadIdentifier
                });

                _db.SaveChanges();
            }
            catch (Exception e)
            {
                // TODO: failed to save? catch the specific type of exception - consider how exceptions should be handled elsewhere in api method
            }

            // return LeadResponse
            return Json(new LeadResponseDto()
            {
                IsBillable = false,
                LeadIdentifier = Guid.Empty,
                LeadStatuses = new List<LeadStatusDto>()
                  {
                      new LeadStatusDto(){}
                  }
            });

            throw new NotImplementedException();
        }

        [HttpPut]
        public async Task<object> AddOrReplaceFileAsync([FromBody] LeadFileDto leadFile)
        {
            // lookup Partner by the ApiToken provided
            var partner = _db.Partner
                .Include(p => p.PartnerType)
                .Where(p => p.ApiToken == leadFile.ApiToken && p.PartnerType.Name == "Pay Per Lead")
                .FirstOrDefault();

            // lookup lead

            // create or update file (blob storage?)

            // return response to caller indicating success or failure
            throw new NotImplementedException();
        }

        [HttpPatch]
        public async Task<object> UpdateLeadAsync([FromBody] LeadRequestDto leadRequest)
        {
            // don't need to support this yet (or maybe ever)
            throw new NotImplementedException();
        }

        [HttpDelete]
        public async Task<object> DeleteLeadAsync(Guid leadIdentifier)
        {
            // don't need to support this yet (or maybe ever)
            throw new NotImplementedException();
        }

    }
}