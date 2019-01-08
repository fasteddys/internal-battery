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
        [HttpGet("/api/[controller]/search/{searchQuery}")]
        public IActionResult Search(string searchQuery)
        {
            List<Subscriber> subscribers = _db.Subscriber
                .Include(s => s.SubscriberSkills)
                .ThenInclude(s => s.Skill)
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .Where(s => s.IsDeleted == 0
                && (s.Email.Contains(searchQuery)
                    || s.FirstName.Contains(searchQuery)
                    || s.LastName.Contains(searchQuery)
                    || s.PhoneNumber.Contains(searchQuery)
                    || s.Address.Contains(searchQuery)
                    || s.City.Contains(searchQuery)
                    || s.SubscriberSkills.Where(k => k.Skill.SkillName.Contains(searchQuery)).Any()
                    || s.SubscriberWorkHistory.Where(w => w.JobDecription.Contains(searchQuery)).Any()
                    || s.SubscriberWorkHistory.Where(w => w.Title.Contains(searchQuery)).Any()
                    || s.SubscriberWorkHistory.Where(w => w.Company.CompanyName.Contains(searchQuery)).Any()
                    || s.SubscriberEducationHistory.Where(e => e.EducationalDegree.Degree.Contains(searchQuery)).Any()
                    || s.SubscriberEducationHistory.Where(e => e.EducationalDegreeType.DegreeType.Contains(searchQuery)).Any()
                    || s.SubscriberEducationHistory.Where(e => e.EducationalInstitution.Name.Contains(searchQuery)).Any())
                )
                .ToList();

            return Json(_mapper.Map<List<SubscriberDto>>(subscribers));

            return View();
        }
        
        [HttpGet("/api/[controller]/search")]
        public IActionResult Search()
        {
            List<Subscriber> subscribers = _db.Subscriber
                .Where(s => s.IsDeleted == 0)
                .Include(s => s.SubscriberSkills)
                .ThenInclude(s => s.Skill)
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .ToList();

            return Json(_mapper.Map<List<SubscriberDto>>(subscribers));

            return View();
        }

        // todo: specify a policy-based authorization check (using roles stored in azure ad b2c if possible)
        // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-2.2
        // [Authorize] 
        [HttpGet("{subscriberGuid}")]
        public IActionResult Get(Guid subscriberGuid)
        {
            Subscriber subscriber = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                .Include(s => s.State).ThenInclude(c => c.Country)
                .Include(s => s.SubscriberSkills).ThenInclude(ss => ss.Skill)
                .Include(s => s.Enrollments).ThenInclude(e => e.Course)
                .Include(s => s.SubscriberWorkHistory).ThenInclude(swh => swh.Company)
                .Include(s => s.SubscriberWorkHistory).ThenInclude(swh => swh.CompensationType)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalInstitution)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalDegreeType)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalDegree)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound();
            else
                return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }
    }
}
