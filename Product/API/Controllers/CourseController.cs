using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using UpDiddyApi.Models;
using UpDiddyLib;
using UpDiddyLib.MessageQueue;
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi.Controllers
{
 
    [ApiController]
    public class CourseController : ControllerBase
    {


        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly CCQueue  _queue = null;
        public CourseController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {            
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _queue = new CCQueue("ccmessagequeue", _queueConnection);
        }



        // Post: api/course/vendor/coursecode
        // TODO make Authorized 
        // [Authorize]
        // TODO make post 
        [HttpGet]
        [Route("api/[controller]/PurchaseCourse/{VendorCode}/{CourseCode}")]
        public IActionResult PurchaseCourse(string VendorCode, string CourseCode)
        {
            // 1) Create Enrollment record
            // 2) Queue Purchase Course Message 
            var Msg = new EnrollmentMessage
            {
                CourseCode = CourseCode,
                VendorGuid = VendorCode,
                Nonce = 1030304343.00
            };
            _queue.EnQueue<EnrollmentMessage>(Msg);

            return Ok(CourseCode);
        }



        // Post: api/course/vendor/coursecode
        // TODO make Authorized 
        // [Authorize]
        // TODO make post 
        [HttpGet]
        [Route("api/[controller]/EnrollStudent/{EnrollmentGuid}")]
        public IActionResult EnrollStudent(string EnrollmentGuid)
        {

            // 1) Call WOZ API if users does not have an exeter ID
            // 2) Return OK
            return Ok();
        }

        // Post: api/course/vendor/coursecode
        // TODO make Authorized 
        // [Authorize]
        // TODO make post 
        [HttpGet]
        [Route("api/[controller]/CreateSection/{EnrollmentGuid}")]
        public IActionResult CreateSection(string EnrollmentGuid)
        {
            // 1) Call WOZ to see if section exists if not create it 
            // 2) Return OK
            return Ok();
        }

        // Post: api/course/vendor/coursecode
        // TODO make Authorized 
        // [Authorize]
        // TODO make post 
        [HttpGet]
        [Route("api/[controller]/EnrollStudentInSection/{EnrollmentGuid}")]
        public IActionResult EnrollStudentInSection(string EnrollmentGuid)
        {
            // 1) 
            // 2) Return OK
            return Ok();
        }




    }
}