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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using UpDiddyApi.Authorization.APIGateway;
using System.Security.Claims;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class LeadController : Controller
    {
        // TODO: future consideration - we may want to implement some kind of tracing that stores the raw message sent to our system to assist vendors in troubleshooting lead submissions
        private bool _isBillable = true;
        private List<LeadStatusDto> _leadStatuses = null;
        private Guid? _leadIdentifier = null;
        private List<LeadStatusDto> _allLeadStatuses = null;
        private PartnerContact _partnerContact = null;
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
            _leadStatuses = new List<LeadStatusDto>();
            _allLeadStatuses =
                new LeadStatusFactory(_db, _configuration, _syslog, _distributedCache)
                .GetAllLeadStatuses()
                .Select(ls => new LeadStatusDto()
                {
                    LeadStatusId = ls.LeadStatusId,
                    Message = ls.Description,
                    Severity = ls.Severity.ToString(),
                    Name = ls.Name
                })
                .ToList();
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
        [Authorize(AuthenticationSchemes = APIGatewayDefaults.AuthenticationScheme)]
        public async Task<IActionResult> InsertLeadAsync([FromBody] LeadRequestDto leadRequest)
        {
            // abort if the lead file dto is null
            if (leadRequest == null)
                return StatusCode((int)HttpStatusCode.BadRequest, "Missing or invalid parameters");

            // verify that an apiToken was passed from Azure API
            string apiToken = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrWhiteSpace(apiToken))
                return StatusCode((int)HttpStatusCode.Unauthorized, "No ApiToken provided");

            // lookup Partner by the ApiToken provided
            var partner = _db.Partner
                .Include(p => p.PartnerType)
                .Where(p => p.ApiToken == apiToken && p.PartnerType.Name == "Pay Per Lead")
                .FirstOrDefault();

            // abort immediately if we do not recognize the Partner
            if (partner == null)
                return StatusCode((int)HttpStatusCode.Unauthorized, "Invalid ApiToken provided");

            // perform business rule checks
            DupeCheck(leadRequest);
            RequiredFields<LeadRequestDto>(leadRequest);
            TestFlag(leadRequest);

            // email address is the only required field that prevents us from saving a lead (not nullable on dbo.Contact)
            if (leadRequest.EmailAddress != null)
            {
                // save lead to db with statuses
                SaveLead(leadRequest, partner);
            }

            // construct the lead response
            return Json(new LeadResponseDto()
            {
                IsBillable = _isBillable,
                LeadIdentifier = _leadIdentifier,
                LeadStatuses = _leadStatuses
            });
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
        [DisableRequestSizeLimit]
        [Authorize(AuthenticationSchemes = APIGatewayDefaults.AuthenticationScheme)]
        [Route("{leadIdentifier}/file")]
        public async Task<object> AddOrReplaceFileAsync(Guid leadIdentifier, [FromBody] LeadFileDto leadFile)
        {
            // abort if the lead file dto is ull
            if (leadFile == null)
                return StatusCode((int)HttpStatusCode.BadRequest, "Missing or invalid parameters");

            // verify that an apiToken was passed from Azure API
            string apiToken = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrWhiteSpace(apiToken))
                return StatusCode((int)HttpStatusCode.Unauthorized, "No ApiToken provided");

            // lookup Partner by the ApiToken provided
            var partner = _db.Partner
                .Include(p => p.PartnerType)
                .Where(p => p.ApiToken == apiToken && p.PartnerType.Name == "Pay Per Lead")
                .FirstOrDefault();

            // abort if we do not recognize the Partner by their token
            if (partner == null)
                return StatusCode((int)HttpStatusCode.Unauthorized, "Invalid ApiToken provided");

            // lookup lead
            var partnerContact = _db.PartnerContact.Where(pc => pc.IsDeleted == 0 && pc.PartnerContactGuid == leadFile.LeadIdentifier).FirstOrDefault();
            if (partnerContact == null)
                return StatusCode((int)HttpStatusCode.NotFound, "Lead not found");
            else
                _leadIdentifier = partnerContact.PartnerContactGuid;

            // required fields are necessary to store the file; skip if validation fails
            if (RequiredFields<LeadFileDto>(leadFile))
            {
                // add or update the partner contact file
                SaveFile(leadFile, partnerContact);
            }

            // construct the lead response 
            return Json(new LeadResponseDto()
            {
                IsBillable = _isBillable,
                LeadIdentifier = _leadIdentifier,
                LeadStatuses = _leadStatuses
            });
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

        #region Private Methods

        /// <summary>
        /// Performs a simplistic dupe check (name or email, infinite dupe window) on a lead. If a lead is identified as a duplicate, we update
        /// the lead statuses collection accordingly and set the lead as non-billable.
        /// </summary>
        /// <param name="leadRequest"></param>
        private void DupeCheck(LeadRequestDto leadRequest)
        {
            var email = new SqlParameter("@Email", SqlDbType.NVarChar);
            email.Value = (object)leadRequest.EmailAddress ?? DBNull.Value;
            var phone = new SqlParameter("@Phone", SqlDbType.NVarChar);
            phone.Value = (object)leadRequest.MobilePhone ?? DBNull.Value;
            var isDupe = new SqlParameter { ParameterName = "@IsDupe", SqlDbType = SqlDbType.Bit, Size = 1, Direction = ParameterDirection.Output };
            var spParams = new object[] { email, phone, isDupe };
            var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Get_LeadDupeCheck] 
                    @Email,
                    @Phone,
	                @IsDupe OUTPUT", spParams);
            if (Convert.ToBoolean(isDupe.Value))
            {
                _leadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Duplicate").FirstOrDefault());
                _isBillable = false;
            }
        }

        /// <summary>
        /// Checks required fields using ValidationContext for a lead. If a lead is identified as missing required fields, we update the lead
        /// statuses collection accordingly and set the lead as non-billable. Additional detail for each validation message is added to the 
        /// lead status description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToValidate"></param>
        /// <returns>True if passed validation, otherwise False</returns>
        private bool RequiredFields<T>(T objectToValidate)
        {
            var context = new ValidationContext(objectToValidate, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(objectToValidate, context, results, validateAllProperties: true);
            if (!isValid)
            {
                var requiredFieldsLeadStatus = _allLeadStatuses.Where(ls => ls.Name == "Required Fields").FirstOrDefault();
                var requiredFieldsDetails = string.Join(", ", results.Select(r => r.ErrorMessage).ToArray()).Trim().TrimEnd(',');
                requiredFieldsLeadStatus.Message += $": {requiredFieldsDetails}";
                _leadStatuses.Add(requiredFieldsLeadStatus);
                _isBillable = false;
            }
            return isValid;
        }

        /// <summary>
        /// Checks to see if the IsTest flag was set to true. If it was, we update the lead statuses collection accordingly and set the lead 
        /// as non-billable. 
        /// </summary>
        /// <param name="leadRequest"></param>
        private void TestFlag(LeadRequestDto leadRequest)
        {
            if (leadRequest.IsTest.HasValue && leadRequest.IsTest.Value)
            {
                _leadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Test").FirstOrDefault());
                _isBillable = false;
            }
        }

        /// <summary>
        /// Saves (and potentially overwrites) the file to the db along with any associated lead statuses. Any lead statuses associated with 
        /// existing files will be logically deleted. If any exceptions occur, the system error lead status will be added to the lead file response.
        /// </summary>
        /// <param name="leadFile"></param>
        /// <param name="partner"></param>
        private void SaveFile(LeadFileDto leadFile, PartnerContact partnerContact)
        {
            try
            {
                // check to see if there is an existing file for this partner contact with the same name
                var partnerContactFile = _db.PartnerContactFile.Where(pcf => pcf.IsDeleted == 0 && pcf.Name == leadFile.Name).FirstOrDefault();

                if (partnerContactFile == null)
                {
                    // if lead is still billable at this point, add a lead status indicating this is a new file
                    if (_isBillable)
                        _leadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Inserted").FirstOrDefault());

                    // set up lead statuses to be saved on the partner contact file
                    List<PartnerContactFileLeadStatus> partnerContactFileLeadStatuses =
                        _leadStatuses.Select(ls => new PartnerContactFileLeadStatus()
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            IsDeleted = 0,
                            LeadStatusId = ls.LeadStatusId,
                            PartnerContactFileLeadStatusGuid = Guid.NewGuid()
                        })
                        .ToList();

                    // create a new partner contact file
                    _db.Add(new PartnerContactFile()
                    {
                        Base64EncodedData = leadFile.Base64EncodedData,
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsBillable = _isBillable,
                        IsDeleted = 0,
                        MimeType = leadFile.MimeType,
                        Name = leadFile.Name,
                        PartnerContactId = partnerContact.PartnerContactId,
                        PartnerContactFileGuid = Guid.NewGuid(),
                        PartnerContactFileLeadStatuses = partnerContactFileLeadStatuses
                    });
                }
                else
                {
                    // if lead is still billable at this point, add a lead status indicating this is a new file
                    if (_isBillable)
                        _leadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Updated").FirstOrDefault());

                    // set up lead statuses to be saved on the partner contact file
                    List<PartnerContactFileLeadStatus> partnerContactFileLeadStatuses =
                        _leadStatuses.Select(ls => new PartnerContactFileLeadStatus()
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            IsDeleted = 0,
                            LeadStatusId = ls.LeadStatusId,
                            PartnerContactFileLeadStatusGuid = Guid.NewGuid()
                        })
                        .ToList();

                    // mark old partner contact lead statuses as deleted
                    _db.PartnerContactFileLeadStatus
                        .Where(pcfls => pcfls.IsDeleted == 0 && pcfls.PartnerContactFileId == partnerContactFile.PartnerContactFileId)
                        .ToList()
                        .ForEach(pcfls =>
                        {
                            pcfls.IsDeleted = 1;
                            pcfls.ModifyDate = DateTime.UtcNow;
                            pcfls.ModifyGuid = Guid.Empty;
                        });

                    // update existing file with new values
                    partnerContactFile.Base64EncodedData = leadFile.Base64EncodedData;
                    partnerContactFile.IsBillable = _isBillable;
                    partnerContactFile.MimeType = leadFile.MimeType;
                    partnerContactFile.ModifyDate = DateTime.UtcNow;
                    partnerContactFile.ModifyGuid = Guid.Empty;
                    partnerContactFile.PartnerContactFileLeadStatuses = partnerContactFileLeadStatuses;
                }

                _db.SaveChanges();
            }
            catch (Exception e)
            {
                _isBillable = false;
                _leadStatuses.Clear();
                _leadIdentifier = null;
                _leadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "System Error").FirstOrDefault());
                _syslog.LogError(e, $"LeadController.SaveFile: {e.Message}", leadFile);
            }
        }

        /// <summary>
        /// Saves the lead to the db along with any associated lead statuses. Handles duplicates by email by using the existing contact. 
        /// If any exceptions occur, the system error lead status will be added to the lead response.
        /// </summary>
        /// <param name="leadRequest"></param>
        /// <param name="partner"></param>
        private void SaveLead(LeadRequestDto leadRequest, Partner partner)
        {
            try
            {
                // generate a new lead id
                _leadIdentifier = Guid.NewGuid();

                // if lead is still billable at this point, add a lead status indicating this
                if (_isBillable)
                    _leadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Inserted").FirstOrDefault());

                // look for an existing contact in the system (possibly from a different partner) 
                var contact = _db.Contact.Where(c => c.Email == leadRequest.EmailAddress).FirstOrDefault();
                if (contact == null)
                {
                    // no existing contact; create a new one when adding the partner contact
                    _partnerContact = new PartnerContact()
                    {
                        Contact = new Contact()
                        {
                            ContactGuid = Guid.NewGuid(),
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            Email = leadRequest.EmailAddress,
                            IsDeleted = 0
                        },
                        IsBillable = _isBillable,
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        Metadata = JObject.Parse(JsonConvert.SerializeObject(leadRequest)),
                        PartnerContactGuid = _leadIdentifier,
                        PartnerId = partner.PartnerId,
                        SourceSystemIdentifier = leadRequest.ExternalLeadIdentifier
                    };
                }
                else
                {
                    // existing contact; use the foreign key to specify the association
                    _partnerContact = new PartnerContact()
                    {
                        ContactId = contact.ContactId,
                        IsBillable = _isBillable,
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        Metadata = JObject.Parse(JsonConvert.SerializeObject(leadRequest)),
                        PartnerContactGuid = _leadIdentifier,
                        PartnerId = partner.PartnerId,
                        SourceSystemIdentifier = leadRequest.ExternalLeadIdentifier
                    };
                }

                // create new lead statuses to be saved on the partner contact 
                _leadStatuses.ForEach(ls =>
                {
                    _partnerContact.PartnerContactLeadStatuses.Add(new PartnerContactLeadStatus()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        LeadStatusId = ls.LeadStatusId,
                        PartnerContactLeadStatusGuid = Guid.NewGuid()
                    });
                });

                _db.PartnerContact.Add(_partnerContact);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                _isBillable = false;
                _leadStatuses.Clear();
                _leadIdentifier = null;
                _leadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "System Error").FirstOrDefault());
                _syslog.LogError(e, $"LeadController.SaveLead: {e.Message}", leadRequest);
            }
        }

        #endregion
    }
}