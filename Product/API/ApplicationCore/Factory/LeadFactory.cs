using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto.Marketing;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class LeadFactory : FactoryBase
    {
        private const string CACHE_KEY = "GetAllLeadStatuses";
        private readonly UpDiddyDbContext _db;
        private readonly ILogger _syslog;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private IHangfireService _hangfireService;
        private List<LeadStatusDto> _allLeadStatuses = null;
        private LeadResponseDto _leadResponse = new LeadResponseDto() { IsBillable = true, HttpStatusCode = HttpStatusCode.Accepted, LeadStatuses = new List<LeadStatusDto>() };
        private PartnerContact _partnerContact = null;

        public LeadFactory(UpDiddyDbContext db, IConfiguration configuration, ILogger syslog, IDistributedCache distributedCache, IHangfireService hangfireService) : base(db, configuration, syslog, distributedCache)
        {
            _db = db;
            _distributedCache = distributedCache;
            _syslog = syslog;
            _configuration = configuration;
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
            _hangfireService = hangfireService;
        }

        /// <summary>
        /// Encapsulates all logic for processing a single lead
        /// </summary>
        /// <param name="leadRequest"></param>
        /// <param name="apiToken"></param>
        /// <returns></returns>
        public LeadResponseDto InsertLead(LeadRequestDto leadRequest, string apiToken)
        {
            // abort if the lead request dto is null
            if (leadRequest == null)
            {
                _leadResponse.LeadStatuses.Add(new LeadStatusDto() { Message = "Missing or invalid parameters", Severity = Severity.Rejected.ToString() });
                _leadResponse.HttpStatusCode = HttpStatusCode.BadRequest;
                _leadResponse.IsBillable = false;
                return _leadResponse;
            }

            // verify that an apiToken was passed from Azure API
            if (string.IsNullOrWhiteSpace(apiToken))
            {
                _leadResponse.LeadStatuses.Add(new LeadStatusDto() { Message = "No ApiToken provided", Severity = Severity.Rejected.ToString() });
                _leadResponse.HttpStatusCode = HttpStatusCode.Unauthorized;
                _leadResponse.IsBillable = false;
                return _leadResponse;
            }

            // lookup Partner by the ApiToken provided
            var partner = _db.Partner
                .Include(p => p.PartnerType)
                .Where(p => p.ApiToken == apiToken && p.IsDeleted == 0)
                .FirstOrDefault();

            // abort if we do not recognize the Partner
            if (partner == null)
            {
                _leadResponse.LeadStatuses.Add(new LeadStatusDto() { Message = "Unrecognized Partner", Severity = Severity.Rejected.ToString() });
                _leadResponse.HttpStatusCode = HttpStatusCode.Unauthorized;
                _leadResponse.IsBillable = false;
                return _leadResponse;
            }

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

            return _leadResponse;
        }

        /// <summary>
        /// Encapsulates all logic for processing a single lead file
        /// </summary>
        /// <param name="leadIdentifier"></param>
        /// <param name="leadFile"></param>
        /// <param name="apiToken"></param>
        /// <returns></returns>
        public LeadResponseDto AddOrReplaceFile(Guid leadIdentifier, LeadFileDto leadFile, string apiToken)
        {
            // abort if the lead file dto is null
            if (leadFile == null)
            {
                _leadResponse.LeadStatuses.Add(new LeadStatusDto() { Message = "Missing or invalid parameters", Severity = Severity.Rejected.ToString() });
                _leadResponse.HttpStatusCode = HttpStatusCode.BadRequest;
                _leadResponse.IsBillable = false;
                return _leadResponse;
            }


            // verify that an apiToken was passed from Azure API
            if (string.IsNullOrWhiteSpace(apiToken))
            {
                _leadResponse.LeadStatuses.Add(new LeadStatusDto() { Message = "No ApiToken provided", Severity = Severity.Rejected.ToString() });
                _leadResponse.HttpStatusCode = HttpStatusCode.Unauthorized;
                _leadResponse.IsBillable = false;
                return _leadResponse;
            }

            // lookup Partner by the ApiToken provided
            var partner = _db.Partner
                .Include(p => p.PartnerType)
                .Where(p => p.ApiToken == apiToken && p.PartnerType.Name == "Pay Per Lead" && p.IsDeleted == 0)
                .FirstOrDefault();

            // abort if we do not recognize the Partner
            if (partner == null)
            {
                _leadResponse.LeadStatuses.Add(new LeadStatusDto() { Message = "Unrecognized Partner", Severity = Severity.Rejected.ToString() });
                _leadResponse.HttpStatusCode = HttpStatusCode.Unauthorized;
                _leadResponse.IsBillable = false;
                return _leadResponse;
            }

            // lookup lead
            var partnerContact = _db.PartnerContact.Where(pc => pc.IsDeleted == 0 && pc.PartnerContactGuid == leadIdentifier).FirstOrDefault();
            if (partnerContact == null)
            {
                _leadResponse.LeadStatuses.Add(new LeadStatusDto() { Message = "Lead not found", Severity = Severity.Rejected.ToString() });
                _leadResponse.HttpStatusCode = HttpStatusCode.NotFound;
                _leadResponse.IsBillable = false;
            }
            else
            {
                _leadResponse.LeadIdentifier = partnerContact.PartnerContactGuid;
            }

            // required fields are necessary to store the file; skip if validation fails
            if (RequiredFields<LeadFileDto>(leadFile))
            {
                // add or update the partner contact file
                SaveFile(leadFile, partnerContact);
            }

            return _leadResponse;
        }

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
                _leadResponse.LeadStatuses.Add(
                    _allLeadStatuses
                    .Where(ls => ls.Name == "Duplicate")
                    .FirstOrDefault());
                _leadResponse.IsBillable = false;
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
                _leadResponse.LeadStatuses.Add(requiredFieldsLeadStatus);
                _leadResponse.IsBillable = false;
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
                _leadResponse.LeadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Test").FirstOrDefault());
                _leadResponse.IsBillable = false;
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
                _leadResponse.LeadIdentifier = Guid.NewGuid();

                // if lead is still billable at this point, add a lead status indicating this
                if (_leadResponse.IsBillable)
                    _leadResponse.LeadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Inserted").FirstOrDefault());

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
                        IsBillable = _leadResponse.IsBillable,
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        Metadata = JObject.Parse(JsonConvert.SerializeObject(leadRequest)),
                        PartnerContactGuid = _leadResponse.LeadIdentifier,
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
                        IsBillable = _leadResponse.IsBillable,
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        Metadata = JObject.Parse(JsonConvert.SerializeObject(leadRequest)),
                        PartnerContactGuid = _leadResponse.LeadIdentifier,
                        PartnerId = partner.PartnerId,
                        SourceSystemIdentifier = leadRequest.ExternalLeadIdentifier
                    };
                }

                // simple campaign matching logic based on the presence of a CampaignPartner contact (this may need to be become more flexible in the future)
                var matchingCampaign = _db.CampaignPartner.Where(cp => 
                        cp.Partner.PartnerId == partner.PartnerId
                        && cp.IsDeleted == 0
                        && cp.Campaign.IsDeleted == 0
                        && cp.Campaign.StartDate <= DateTime.UtcNow
                        && (!cp.Campaign.EndDate.HasValue || cp.Campaign.EndDate.Value > DateTime.UtcNow))
                    .OrderByDescending(cp => cp.Campaign.StartDate)
                    .Select(cp => cp.Campaign)
                    .FirstOrDefault();

                // associate the lead with the PPL campaign
                _db.CampaignPartnerContact.Add(
                    new CampaignPartnerContact()
                    {
                        CampaignId = matchingCampaign.CampaignId,
                        CampaignPartnerContactGuid = Guid.NewGuid(),
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        PartnerContact = _partnerContact,
                        IsEmailSent = false
                    });

                // create new lead statuses to be saved on the partner contact 
                _leadResponse.LeadStatuses.ForEach(ls =>
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

                // TODO: identify any business rule failures that would prevent us from sending a welcome email (dupe, missing fields, etc?)
                int verificationFailureLeadStatusId = _allLeadStatuses.Where(ls => ls.Name == "Verification Failure").FirstOrDefault().LeadStatusId;
                _hangfireService.Enqueue<ScheduledJobs>(j => j.ValidateEmailAddress(_partnerContact.PartnerContactGuid.Value, leadRequest.EmailAddress, verificationFailureLeadStatusId));
            }
            catch (Exception e)
            {
                _leadResponse.IsBillable = false;
                _leadResponse.LeadStatuses.Clear();
                _leadResponse.LeadIdentifier = null;
                _leadResponse.LeadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "System Error").FirstOrDefault());
                _syslog.LogError(e, $"LeadFactory.SaveLead: {e.Message}", leadRequest);
            }
        }

        /// <summary>
        /// Saves (and potentially overwrites) the file to the db along with any associated lead statuses. Any lead statuses associated with 
        /// existing files will be logically deleted. If any exceptions occur, the system error lead status will be added to the lead file response.
        /// </summary>
        /// <param name="leadFile"></param>
        /// <param name="partnerContact"></param>
        private void SaveFile(LeadFileDto leadFile, PartnerContact partnerContact)
        {
            try
            {
                // check to see if there is an existing file for this partner contact with the same name
                var partnerContactFile = _db.PartnerContactFile.Where(pcf => pcf.IsDeleted == 0 && pcf.Name == leadFile.FileName).FirstOrDefault();

                if (partnerContactFile == null)
                {
                    // if lead is still billable at this point, add a lead status indicating this is a new file
                    if (_leadResponse.IsBillable)
                        _leadResponse.LeadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Inserted").FirstOrDefault());

                    // set up lead statuses to be saved on the partner contact file
                    List<PartnerContactFileLeadStatus> partnerContactFileLeadStatuses =
                        _leadResponse.LeadStatuses.Select(ls => new PartnerContactFileLeadStatus()
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
                        IsBillable = _leadResponse.IsBillable,
                        IsDeleted = 0,
                        MimeType = leadFile.MimeType,
                        Name = leadFile.FileName,
                        PartnerContactId = partnerContact.PartnerContactId,
                        PartnerContactFileGuid = Guid.NewGuid(),
                        PartnerContactFileLeadStatuses = partnerContactFileLeadStatuses
                    });
                }
                else
                {
                    // if lead is still billable at this point, add a lead status indicating this is a new file
                    if (_leadResponse.IsBillable)
                        _leadResponse.LeadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "Updated").FirstOrDefault());

                    // set up lead statuses to be saved on the partner contact file
                    List<PartnerContactFileLeadStatus> partnerContactFileLeadStatuses =
                        _leadResponse.LeadStatuses.Select(ls => new PartnerContactFileLeadStatus()
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
                    partnerContactFile.IsBillable = _leadResponse.IsBillable;
                    partnerContactFile.MimeType = leadFile.MimeType;
                    partnerContactFile.ModifyDate = DateTime.UtcNow;
                    partnerContactFile.ModifyGuid = Guid.Empty;
                    partnerContactFile.PartnerContactFileLeadStatuses = partnerContactFileLeadStatuses;
                }

                _db.SaveChanges();
            }
            catch (Exception e)
            {
                _leadResponse.IsBillable = false;
                _leadResponse.LeadStatuses.Clear();
                _leadResponse.LeadIdentifier = null;
                _leadResponse.LeadStatuses.Add(_allLeadStatuses.Where(ls => ls.Name == "System Error").FirstOrDefault());
                _syslog.LogError(e, $"LeadController.SaveFile: {e.Message}", leadFile);
            }
        }
    }
}
