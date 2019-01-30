using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
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
            // invoke the tracking asynchronously
            Task.Run(() => ProcessTrackingInformation(Request.Query.Keys.ToDictionary(k => k, k => Request.Query[k])));

            // return the tracking pixel
            return _pixelResponse;
        }

        private void ProcessTrackingInformation(Dictionary<string, StringValues> parameters)
        {
            // look for expected parameters (contact, action, campaign)
            var campaign = parameters.Where(p => p.Key.EqualsInsensitive(Constants.TRACKING_KEY_CAMPAIGN)).Select(p => p.Value).FirstOrDefault().FirstOrDefault();
            var contact = parameters.Where(p => p.Key.EqualsInsensitive(Constants.TRACKING_KEY_CONTACT)).Select(p => p.Value).FirstOrDefault().FirstOrDefault();
            var action = parameters.Where(p => p.Key.EqualsInsensitive(Constants.TRACKING_KEY_ACTION)).Select(p => p.Value).FirstOrDefault().FirstOrDefault();

            // must have all three tracking parameters in order to continue
            if (campaign != null && contact != null && action != null)
            {
                // validate that all parameters are guids
                Guid campaignGuid, contactGuid, actionGuid;
                if (Guid.TryParse(campaign, out campaignGuid) && Guid.TryParse(contact, out contactGuid) && Guid.TryParse(action, out actionGuid))
                {
                    // invoke the Hangfire job to store the tracking information
                    BackgroundJob.Enqueue<ScheduledJobs>(j => j.StoreTrackingInformation(campaignGuid, contactGuid, actionGuid));
                }
            }
        }
    }
}