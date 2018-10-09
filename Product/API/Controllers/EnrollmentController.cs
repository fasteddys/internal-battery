using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;

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

        // Update the status of an enrolllment 
        [HttpPut]
        [Route("api/[controller]/UpdateEnrollmentStatus/{EnrollmentGuid}/{EnrollmentStatus}")]
        public IActionResult UpdateEnrollmentStatus(string EnrollmentGuid, int EnrollmentStatus )
        {
            try
            {
                // Get the Enrollment Object 
                Enrollment Enrollment = _db.Enrollment
                     .Where(t => t.IsDeleted == 0 && t.EnrollmentGuid.ToString() == EnrollmentGuid)
                     .FirstOrDefault();

                if (Enrollment == null)
                    return NotFound();

                // Update the enrollment status and update the modify date 
                Enrollment.EnrollmentStatusId = EnrollmentStatus;
                Enrollment.ModifyDate = DateTime.Now;
                _db.SaveChanges();
                

                return Ok(Enrollment.EnrollmentGuid);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex);
            }
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

    }
}
