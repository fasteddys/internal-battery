using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using AutoMapper.QueryableExtensions;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
    //TODO [Authorize]
    [ApiController]
    public class EnrollmentController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly CCQueue _queue = null;
        public EnrollmentController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _queue = new CCQueue("ccmessagequeue", _queueConnection);
        }

   
        [HttpPost]
        [Route("api/[controller]")]
        public IActionResult Post([FromBody] EnrollmentDto EnrollmentDto)
        {

            try
            {
                Enrollment Enrollment = _mapper.Map<Enrollment>(EnrollmentDto);
                _db.Enrollment.Add(Enrollment);
                _db.SaveChanges();
                return Ok(Enrollment.EnrollmentGuid);
            }
            catch ( Exception ex )
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex);
            }
           
        }

        [HttpGet]
        [Route("api/[controller]/CurrentEnrollments/{SubscriberId}")]
        public IActionResult GetCurrentEnrollmentsForSubscriber(int SubscriberId)
        {
            IList<EnrollmentDto> rval = null;
            rval = _db.Enrollment
                .Where(t => t.IsDeleted == 0 && t.SubscriberId == SubscriberId)
                .ProjectTo<EnrollmentDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(rval);
        }
    }
}
