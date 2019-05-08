using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class TrackingController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;
        private readonly FileContentResult _pixelResponse;

        public TrackingController(UpDiddyDbContext db, ILogger<TrackingController> sysLog, FileContentResult pixelResponse)
        {
            _db = db;
            _syslog = sysLog;
            _pixelResponse = pixelResponse;
        }

        [HttpGet]
        [Route("api/[controller]")]
        public IActionResult Get()
        {

            // gather all query parameters
            var parameters = Request.Query.Keys.ToDictionary(k => k, k => Request.Query[k]);

            // serialize all headers into json
            var headers = JsonConvert.SerializeObject(Request.Headers, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // invoke the tracking asynchronously
            Task.Run(() => ProcessTrackingInformation(parameters, headers));

            // return the tracking pixel
            return _pixelResponse;
        }

        [HttpGet]
        [Route("api/[controller]/{campaignGuid?}/{partnerContactGuid?}/{actionGuid?}")]
        public IActionResult Get(string campaignGuid, string partnerContactGuid, string actionGuid)
        {
            // construct tracking parameters
            Dictionary<string, StringValues> parameters = new Dictionary<string, StringValues>();
            parameters.Add(Constants.TRACKING_KEY_CAMPAIGN, campaignGuid);
            parameters.Add(Constants.TRACKING_KEY_PARTNER_CONTACT, partnerContactGuid);
            parameters.Add(Constants.TRACKING_KEY_ACTION, actionGuid);
            // add the phase if one has been specified, otherwise leave it null since ther is logic 
            // that depends it being null if un-specified
            string phase = Request.Query[Constants.TRACKING_KEY_CAMPAIGN_PHASE].ToString();
            if (!string.IsNullOrEmpty(phase))
                parameters.Add(Constants.TRACKING_KEY_CAMPAIGN_PHASE, phase);
            // serialize all headers into json
            var headers = JsonConvert.SerializeObject(Request.Headers, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // invoke the tracking asynchronously
            Task.Run(() => ProcessTrackingInformation(parameters, headers));

            // return the tracking pixel
            return _pixelResponse;
        }

        private void ProcessTrackingInformation(Dictionary<string, StringValues> parameters, string headers)

        {

            // look for expected parameters (contact, action, campaign, campaignPhase)
            var campaign = parameters.Where(p => p.Key.EqualsInsensitive(Constants.TRACKING_KEY_CAMPAIGN)).Select(p => p.Value).FirstOrDefault().FirstOrDefault();
            var partnerContact = parameters.Where(p => p.Key.EqualsInsensitive(Constants.TRACKING_KEY_PARTNER_CONTACT)).Select(p => p.Value).FirstOrDefault().FirstOrDefault();
            var action = parameters.Where(p => p.Key.EqualsInsensitive(Constants.TRACKING_KEY_ACTION)).Select(p => p.Value).FirstOrDefault().FirstOrDefault();
            var campaignPhase = parameters.Where(p => p.Key.EqualsInsensitive(Constants.TRACKING_KEY_CAMPAIGN_PHASE)).Select(p => p.Value).FirstOrDefault().FirstOrDefault();

            // must have all three tracking parameters in order to continue
            if (campaign != null && partnerContact != null && action != null)
            {
                // validate that all parameters are guids
                Guid campaignGuid, partnerContactGuid, actionGuid;
                if (Guid.TryParse(campaign, out campaignGuid) && Guid.TryParse(partnerContact, out partnerContactGuid) && Guid.TryParse(action, out actionGuid))
                {
                    // invoke the Hangfire job to store the tracking information
                    BackgroundJob.Enqueue<ScheduledJobs>(j => j.StoreTrackingInformation(campaignGuid, partnerContactGuid, actionGuid, campaignPhase, headers));
                }
            }
        }

        /// <summary>
        /// Generic method which takes in
        /// </summary>
        /// <param name="ActorGuid"></param>
        /// <param name="ActionGuid"></param>
        /// <param name="EntityGuid"></param>
        /// <returns></returns>
        [HttpGet("api/[controller]/recruiter/{ActorGuid}/{ActionGuid}/{JobApplicationGuid}")]
        public IActionResult Track(Guid ActorGuid, Guid ActionGuid, Guid JobApplicationGuid)
        {
            Task.Run(() => ProcessRecruiterTrackingInformation(ActorGuid, ActionGuid, JobApplicationGuid));
            return _pixelResponse;
        }

        private void ProcessRecruiterTrackingInformation(Guid ActorGuid, Guid ActionGuid, Guid JobApplicationGuid)
        {
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.StoreRecruiterTrackingInformation(ActorGuid, ActionGuid, JobApplicationGuid));
        }
    }
}