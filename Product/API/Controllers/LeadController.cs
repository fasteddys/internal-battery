using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using UpDiddyLib.Dto.Marketing;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.ApplicationCore.Factory;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class LeadController : Controller
    {
        private bool isBillable = false;
        private readonly UpDiddyDbContext _db;
        private readonly ILogger _syslog;
        private ICloudStorage _cloudStorage;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;

        public LeadController(UpDiddyDbContext db,
            ILogger<LeadController> sysLog,
            ICloudStorage cloudStorage,
            IDistributedCache distributedCache,
            IConfiguration configuration)
        {
            this._db = db;
            this._syslog = sysLog;
            this._cloudStorage = cloudStorage;
            this._configuration = configuration;
        }
        
        /// <summary>
        /// This endpoint is to be used in conjunction with Azure API to facilitate the import of
        /// leads from third party companies. The prospective lead is validated for business rules,
        /// stored in the CareerCircle system, and a response is returned to the caller which indicates
        /// the status of the lead along with an unique identifier for the lead.
        /// </summary>
        /// <param name="leadRequest"></param>
        /// <returns></returns>
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

            // retrieve lead statuses that will be used during lead processing
            var allLeadStatuses = new LeadStatusFactory(_db, _configuration, _syslog, _distributedCache).GetAllLeadStatuses();

            // by default, a lead is accepted
            bool isBillable = true;

            // TODO: do we need this? lead statuses which will be stored in the db go here
            List<PartnerContactLeadStatus> partnerContactLeadStatuses = new List<PartnerContactLeadStatus>();

            // TODO: consider using only this... get rid of the above list?
            var leadStatuses = new List<LeadStatus>();

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
                leadStatuses.Add(allLeadStatuses.Where(ls => ls.Name == "Duplicate").FirstOrDefault());
                isBillable = false;
            }
            
            // check required fields using ValidationContext
            var context = new ValidationContext(leadRequest, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(leadRequest, context, results, validateAllProperties: true);
            if (!isValid)
            {
                var requiredFieldsLeadStatus = allLeadStatuses.Where(ls => ls.Name == "Required Fields").FirstOrDefault();
                // add details for each failed validation message to the required fields lead status
                var requiredFieldsDetails = string.Join(", ", results.Select(r => r.ErrorMessage).ToArray()).Trim().TrimEnd(',');
                requiredFieldsLeadStatus.Description += $": {requiredFieldsDetails}";
                leadStatuses.Add(requiredFieldsLeadStatus);
                isBillable = false;
            }

            // check test flag
            if (leadRequest.IsTest.HasValue && leadRequest.IsTest.Value)
            {
                leadStatuses.Add(allLeadStatuses.Where(ls => ls.Name == "Test").FirstOrDefault());
                isBillable = false;
            }

            // TODO: do we want to have a try/catch here? elsewhere?
            try
            {
                // look for an existing contact in the system (possibly from a different partner) 
                var contact = _db.Contact.Where(c => c.Email == leadRequest.EmailAddress).FirstOrDefault();
                if(contact == null)
                {
                    contact = new Contact()
                    {
                        ContactGuid = Guid.NewGuid(),
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        Email = leadRequest.EmailAddress,
                        IsDeleted = 0
                    };
                }

                var partnerContact = new PartnerContact()
                {
                    Contact = contact,
                    IsBillable = isBillable,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0,
                    Metadata = JObject.Parse(JsonConvert.SerializeObject(leadRequest)),
                    PartnerContactGuid = Guid.NewGuid(),
                    PartnerId = partner.PartnerId,
                    SourceSystemIdentifier = leadRequest.ExternalLeadIdentifier
                };

                _db.PartnerContact.Add(partnerContact);

                var result = _db.SaveChanges();


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

            // TODO: future consideration - we may want to implement some kind of tracing that stores the raw message sent to our system to assist vendors in troubleshooting lead submissions
            throw new NotImplementedException();
        }

        /// <summary>
        /// This endpoint is to be used in conjunction with Azure API to facilitate the import of
        /// files associated with existing leads from third party companies. A valid existing lead
        /// identifier must be supplied along with a file. If a file with the same name already 
        /// exists, it will be overwritten. A response is returned to the caller which indicates
        /// the status of the file operation.
        /// </summary>
        /// <param name="leadRequest"></param>
        /// <returns></returns>
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

        [HttpGet]
        public async Task<IActionResult> GetLeadAsync(Guid leadIdentifier)
        {
            // don't need to support this yet (or maybe ever)
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