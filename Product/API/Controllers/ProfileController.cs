using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using System;
using System.Linq;

namespace UpDiddyApi.Controllers
{


    [ApiController]
    public class ProfileController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly CCQueue _queue = null;
        public ProfileController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("api/[controller]/Update")]
        public IActionResult Update([FromBody] SubscriberDto Subscriber)
        {

            try
            {
                Subscriber subscriber = _db.Subscriber
                     .Where(t => t.IsDeleted == 0 && t.SubscriberGuid.Equals(Subscriber.SubscriberGuid))
                     .FirstOrDefault();

                if (subscriber != null)
                {
                    if (!string.IsNullOrEmpty(Subscriber.FirstName))
                    {
                        subscriber.FirstName = Subscriber.FirstName;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.LastName))
                    {
                        subscriber.LastName = Subscriber.LastName;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.Address))
                    {
                        subscriber.Address = Subscriber.Address;
                    }
                    if (!string.IsNullOrEmpty(Subscriber.PhoneNumber))
                    {
                        subscriber.PhoneNumber = Subscriber.PhoneNumber;
                    }
                }
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
            }
            return Ok();

        }
    }

}