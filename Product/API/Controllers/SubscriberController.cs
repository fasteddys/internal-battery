using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Controllers  
{
    [ApiController]    
 
    public class SubscriberController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        protected internal ISysLog _syslog = null;

        public SubscriberController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            _db = db;
            _mapper = mapper;
            _syslog = new SysLog(configuration, sysemail, serviceProvider);
        }

        // GET: api/courses
        [HttpGet]
        [Authorize]
        [Route("api/[controller]")]
        public IActionResult Get()
        {

            IList<SubscriberDto> rval = null;
            rval = _db.Subscriber
                .Where(t => t.IsDeleted == 0)
                .ProjectTo<SubscriberDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rval);

        }

        [HttpGet]
        [Authorize]
        [Route("api/[controller]/{SubscriberGuid}")]
        public IActionResult Get(Guid SubscriberGuid)
        {
            Subscriber subscriber = _db.Subscriber
                .Where(t => t.IsDeleted == 0 && t.SubscriberGuid == SubscriberGuid)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound();

            return Ok(_mapper.Map<SubscriberDto>(subscriber));

        }
        
        [HttpGet] 
        [Route("api/[controller]/CountryFromState/{StateId}")]
        public IActionResult CountryFromState(int StateId)
        {
            State state = _db.State
                .Where(t => t.IsDeleted == 0 && t.StateId == StateId)
                .FirstOrDefault();

            Country country = _db.Country
                .Where(t => t.IsDeleted == 0 && t.CountryId == state.CountryId)
                .FirstOrDefault();

            if (country == null)
                return NotFound();

            return Ok(_mapper.Map<CountryDto>(country));

        }

        [HttpGet]
        [Route("api/[controller]/State/{StateId}")]
        public IActionResult State(int StateId)
        {
            State state = _db.State
                .Where(t => t.IsDeleted == 0 && t.StateId == StateId)
                .FirstOrDefault();

            if (state == null)
                return NotFound();

            return Ok(_mapper.Map<StateDto>(state));

        }

        [HttpPost]
        [Authorize]
        [Route("api/[controller]/CreateSubscriber/{SubscriberGuid}/{SubscriberEmail}")]
        public IActionResult NewSubscriber(string SubscriberGuid, string SubscriberEmail)
        {
            Subscriber subscriber = new Subscriber();
            subscriber.SubscriberGuid = Guid.Parse(SubscriberGuid);
            subscriber.Email = SubscriberEmail;
            subscriber.CreateDate = DateTime.Now;
            subscriber.ModifyDate = DateTime.Now;
            subscriber.IsDeleted = 0;
            subscriber.ModifyGuid = Guid.NewGuid();
            subscriber.CreateGuid = Guid.NewGuid();
        
            // Save subscriber to database 
            _db.Subscriber.Add(subscriber);
            _db.SaveChanges();

            return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }


        [HttpPost]
        [Authorize]
        [Route("api/[controller]/CreateSubscriber")]
        public IActionResult CreateSubscriber( [FromBody] SubscriberCreateDto NewSubscriber)
        {
            Subscriber subscriber = new Subscriber();
            subscriber.SubscriberGuid = Guid.Parse(NewSubscriber.SubscriberGuid);
            subscriber.Email = NewSubscriber.SubscriberEmail;
            subscriber.CreateDate = DateTime.Now;
            subscriber.ModifyDate = DateTime.Now;
            subscriber.IsDeleted = 0;
            subscriber.ModifyGuid = Guid.NewGuid();
            subscriber.CreateGuid = Guid.NewGuid();

            // Save subscriber to database 
            _db.Subscriber.Add(subscriber);
            _db.SaveChanges();

            return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }
    }
}
