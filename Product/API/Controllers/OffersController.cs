using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddyApi.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using System.IO;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Factory;
using System.Data.SqlClient;
using AutoMapper.QueryableExtensions;
using System.Data;
using System.Web;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;
using Hangfire;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class OffersController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;
        private IB2CGraph _graphClient;
        private IAuthorizationService _authorizationService;
        private ICloudStorage _cloudStorage;
        private ISysEmail _sysEmail;

        public OffersController(UpDiddyDbContext db,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<SubscriberController> sysLog,
            IDistributedCache distributedCache,
            IB2CGraph client,
            ICloudStorage cloudStorage,
            IAuthorizationService authorizationService,
            ISysEmail sysEmail)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _graphClient = client;
            _cloudStorage = cloudStorage;
            _authorizationService = authorizationService;
            _sysEmail = sysEmail;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Offer> offers = _db.Offer
                .Where(s => s.IsDeleted == 0)
                .Include(s => s.Partner)
                .Select(s => new Offer
                {
                     OfferId = s.OfferId,
                     OfferGuid = s.OfferGuid,
                     PartnerId = s.PartnerId,
                     Name = s.Name,
                     Description = s.Description,
                     Disclaimer = s.Disclaimer,
                     Code = null,
                     Url = s.Url,
                     StartDate = s.StartDate,
                     EndDate = s.EndDate,
                     Partner = s.Partner
                })
                .ToList();


            return Ok(offers);
        }

        [Authorize]
        [HttpGet("{OfferGuid}")]
        public async Task<IActionResult> GetOffer(Guid OfferGuid)
        {
            if (User == null)
                return Unauthorized();

            Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (!SubscriberFactory.IsEligibleForOffers(_db, loggedInUserGuid, _syslog, _mapper, User.Identity))
                return Unauthorized();

            Offer offer = _db.Offer
                .Where(s => s.IsDeleted == 0 && s.OfferGuid == OfferGuid)
                .FirstOrDefault();
            return Ok(offer); 
        }

        [Authorize]
        [HttpPost("{OfferGuid}/claim")]
        public async Task<IActionResult> ClaimOffer(Guid OfferGuid)
        {
            if (User == null)
                return Unauthorized();

            Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, loggedInUserGuid);
            if (subscriber == null)
                return Unauthorized();

            // Do tracking logic here...

            if (!SubscriberFactory.IsEligibleForOffers(_db, loggedInUserGuid, _syslog, _mapper, User.Identity))
                return Unauthorized();

            Offer offer = _db.Offer
                .Where(s => s.IsDeleted == 0 && s.OfferGuid == OfferGuid)
                .Include(s => s.Partner)
                .FirstOrDefault();

            if (offer == null)
                return NotFound();

            BackgroundJob.Enqueue(() =>
                _sysEmail.SendTemplatedEmailAsync(subscriber.Email, _configuration["SysEmail:TemplateIds:SubscriberOffer-Redemption"], offer, null));

            return Ok(offer);
        }

        
    }
}
