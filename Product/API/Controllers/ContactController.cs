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
        
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/import/{partnerGuid}/{cacheKey}")]
        public IActionResult ImportContacts(Guid partnerGuid, string cacheKey)
        {
            // todo: change endpoint to return JSON object of type ImportValidationSummaryDto

            // todo: exception handling?
            if (partnerGuid == null || partnerGuid == Guid.Empty || cacheKey == null)
                return BadRequest();

            var cachedContactsForImport = _distributedCache.GetString(cacheKey);

            if (cachedContactsForImport != null)
            {
                List<ContactDto> contacts = JsonConvert.DeserializeObject<List<ContactDto>>(cachedContactsForImport);

                // replace this with parallel for each after done w/ debugging
                foreach (var contact in contacts)
                {
                    var test = ImportContactAsync(partnerGuid, contact);
                }


                //Parallel.ForEach(contacts, (contact) =>
                //{
                //    // aggregate all import actions in parallel, best way?
                //    ImportContact(partnerGuid, contact);
                //});

                // todo: group import actions by ImportBehavior and Reason, calculate 'Count'
            }

            return Ok();
        }

        /// <summary>
        /// Handles the import of a contact into the system. This process evaluates whether the contact already exists and handles the create and update operation
        /// accordingly. Information is returned to the caller indicating what action was taken (Nothing, Insert, Update, Error) and a corresponding reason (for errors).
        /// </summary>
        /// <param name="partnerGuid">Identifies the partner for which the contact will be inserted or updated</param>
        /// <param name="contact">The contact to be inserted or updated (based on the partner specified)</param>
        /// <returns></returns>
        private async Task<ImportActionDto> ImportContactAsync(Guid partnerGuid, ContactDto contact)
        {
            ImportActionDto importAction = new ImportActionDto()
            {
                ImportBehavior = ImportBehavior.Nothing,
                Reason = null
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

                var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Import_Contact] 
                    @PartnerGuid,
                    @Email,
                    @SourceSystemIdentifier,
	                @Metadata,
                    @ImportAction OUTPUT,
                    @Reason OUTPUT", spParams);

                // transform sql output parameters into an ImportActionDto response
                importAction.ImportBehavior = (ImportBehavior) Enum.Parse(typeof(ImportBehavior), sqlImportAction.Value.ToString());
                importAction.Reason = sqlReason.Value == DBNull.Value ? null : sqlReason.Value.ToString();
            }
            catch(Exception e)
            {
                _syslog.LogError($"Exception occurred in ContactController.ImportContact: {e.Message}");
                importAction.Reason = e.Message;
                importAction.ImportBehavior = ImportBehavior.Error;
            }

            return await Task.FromResult<ImportActionDto>(importAction);
        }
    }
}
