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
using UpDiddyApi.Helpers;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class LeadController : Controller
    {
        private readonly ILogger _syslog;
        private ICloudStorage _cloudStorage;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private LeadFactory _leadFactory;

        public LeadController(UpDiddyDbContext db,
            ILogger<LeadController> sysLog,
            ICloudStorage cloudStorage,
            IDistributedCache distributedCache,
            IConfiguration configuration)
        {
            this._syslog = sysLog;
            this._cloudStorage = cloudStorage;
            this._configuration = configuration;
            _leadFactory = new LeadFactory(db, configuration, sysLog, distributedCache);
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
            var apiToken = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var leadResponse = _leadFactory.InsertLead(leadRequest, apiToken);
            return new JsonResultWithStatus(leadResponse, leadResponse.HttpStatusCode);
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
            string apiToken = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var leadResponse = _leadFactory.AddOrReplaceFile(leadIdentifier, leadFile, apiToken);
            return new JsonResultWithStatus(leadResponse, leadResponse.HttpStatusCode);
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
 