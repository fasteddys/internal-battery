using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore;
using UpDiddyLib.Helpers;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hangfire;
using UpDiddyApi.Workflow;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Concurrent;

namespace UpDiddyApi.Controllers
{
    public class ContactController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected readonly ILogger _syslog = null;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ISysEmail _sysemail;
        private readonly IDistributedCache _distributedCache;

        public ContactController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, ILogger<CourseController> syslog, IDistributedCache distributedCache)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _syslog = syslog;
            _httpClientFactory = httpClientFactory;
            _sysemail = sysemail;
            _distributedCache = distributedCache;
        }

        [HttpGet("api/[controller]/{contactGuid}")]
        public IActionResult Get(Guid ContactGuid)
        {
            ContactDto rval = null;
            rval = _db.Contact
                .Where(t => t.IsDeleted == 0 && t.ContactGuid == ContactGuid)
                .ProjectTo<ContactDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
            if (rval == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(rval);
            }
        }
        /// <summary>
        /// Handles the list of contacts to be imported into the system. This method retrieves the list of contacts from the cache and processes 
        /// them individually. This is intentionally so that one error does not prevent the rest of the contacts from being processed. 
        /// </summary>
        /// <param name="partnerGuid">Identifies the partner for which the contact will be inserted or updated</param>
        /// <param name="cacheKey">The key in redis which uniquely identifies the list of contacts to be imported</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/import/{partnerGuid}/{cacheKey}")]
        public IActionResult ImportContacts(Guid partnerGuid, string cacheKey)
        {
            // define the variable that will be returned to the caller
            List<ImportActionDto> importActions = new List<ImportActionDto>();

            // if these values aren't defined, return a bad request response
            if (partnerGuid == null || partnerGuid == Guid.Empty || cacheKey == null)
                return BadRequest();

            // retrieve the contacts to be imported from the cache
            var cachedContactsForImport = _distributedCache.GetString(cacheKey);

            if (cachedContactsForImport == null)
            {
                // if there is no corresponding item in the cache, return a not found response
                return NotFound();
            }
            else
            {
                // deserialize the value from the cache into a list of contacts
                List<ContactDto> contacts = JsonConvert.DeserializeObject<List<ContactDto>>(cachedContactsForImport);

                // parallel for each was causing problems with the dbcontext, do not use it
                foreach (var contact in contacts)
                {
                    importActions.Add(ImportContact(partnerGuid, contact));
                }

                // group all import actions by behavior and reason with a count for each
                importActions = importActions
                    .GroupBy(ivr => new { ivr.ImportBehavior, ivr.Reason })
                    .Select(group => new ImportActionDto()
                    {
                        ImportBehavior = group.Key.ImportBehavior,
                        Reason = group.Key.Reason,
                        Count = group.Count()
                    })
                    .ToList();
            }
            return Ok(importActions);
        }

        /// <summary>
        /// Handles the import of a contact into the system. This method evaluates whether the contact already exists and handles the create and update operation
        /// accordingly. Information is returned to the caller indicating what action was taken (Nothing, Insert, Update, Error) and a corresponding reason (for errors).
        /// </summary>
        /// <param name="partnerGuid">Identifies the partner for which the contact will be inserted or updated</param>
        /// <param name="contact">The contact to be inserted or updated (based on the partner specified)</param>
        /// <returns></returns>
        private ImportActionDto ImportContact(Guid partnerGuid, ContactDto contact)
        {
            // default value if no action is taken
            ImportActionDto importAction = new ImportActionDto()
            {
                ImportBehavior = ImportBehavior.Ignored,
                Reason = "Something went wrong!"
            };

            try
            {
                // define all stored procedure parameters (input and output)
                var sqlPartnerGuid = new SqlParameter { ParameterName = "@PartnerGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = partnerGuid };
                var sqlEmail = new SqlParameter { ParameterName = "@Email", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Size = 450, Value = contact.Email };
                var sqlSourceSystemIdentifier = new SqlParameter { ParameterName = "@SourceSystemIdentifier", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Size = 1000, Value = contact.SourceSystemIdentifier };
                var sqlMetadata = new SqlParameter { ParameterName = "@Metadata", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Size = 5000, Value = JsonConvert.SerializeObject(contact.Metadata) };
                var sqlImportAction = new SqlParameter { ParameterName = "@ImportAction", SqlDbType = SqlDbType.NVarChar, Size = 10, Direction = ParameterDirection.Output };
                var sqlReason = new SqlParameter { ParameterName = "@Reason", SqlDbType = SqlDbType.NVarChar, Size = 500, Direction = ParameterDirection.Output };
                var spParams = new object[] { sqlPartnerGuid, sqlEmail, sqlSourceSystemIdentifier, sqlMetadata, sqlImportAction, sqlReason };

                // invoke the stored procedure
                var rowsAffected = _db.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Import_Contact] 
                    @PartnerGuid,
                    @Email,
                    @SourceSystemIdentifier,
	                @Metadata,
                    @ImportAction OUTPUT,
                    @Reason OUTPUT", spParams);

                // transform sql output parameters into an ImportActionDto response
                importAction.ImportBehavior = (ImportBehavior)Enum.Parse(typeof(ImportBehavior), sqlImportAction.Value.ToString());
                importAction.Reason = sqlReason.Value == DBNull.Value ? null : sqlReason.Value.ToString();
                
            }
            catch (Exception e)
            {
                // log the exception and return with import behavior of error
                _syslog.LogError($"Exception occurred in ContactController.ImportContact: {e.Message}");
                importAction.Reason = e.Message;
                importAction.ImportBehavior = ImportBehavior.Error;
            }

            return importAction;
        }
    }
}
