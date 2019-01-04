using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class SubscriberController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;

        public SubscriberController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ILogger<SubscriberController> sysLog, IDistributedCache distributedCache)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
        }

        // todo: specify a policy-based authorization check (using roles stored in azure ad b2c if possible)
        // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-2.2
        // [Authorize] 
        [HttpGet]
        public IActionResult Get()
        {
            List<Subscriber> subscribers = _db.Subscriber
                .Where(s => s.IsDeleted == 0)
                .Include(s => s.SubscriberSkills)
                .ThenInclude(s => s.Skill)
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .ToList();

            return Json(_mapper.Map<List<SubscriberDto>>(subscribers));
        }

        [HttpGet("{subscriberGuid}")]
        public IActionResult Get(Guid subscriberGuid)
        {
            Subscriber subscriber = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound();
            else
                return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }
    }
}
