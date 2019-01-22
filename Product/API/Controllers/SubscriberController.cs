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

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriberController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;
        private IB2CGraph _graphClient;
        private IAuthorizationService _authorizationService;
        private ICloudStorage _cloudStorage;

        public SubscriberController(UpDiddyDbContext db, 
            IMapper mapper, 
            IConfiguration configuration, 
            ILogger<SubscriberController> sysLog, 
            IDistributedCache distributedCache, 
            IB2CGraph client,
            ICloudStorage cloudStorage,
            IAuthorizationService authorizationService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _graphClient = client;
            _cloudStorage = cloudStorage;
            _authorizationService = authorizationService;
        }
        

        [HttpGet("{subscriberGuid}")]
        public async Task<IActionResult> Get(Guid subscriberGuid)
        {
            // Validate guid for GetSubscriber call
            if (Guid.Empty.Equals(subscriberGuid) || subscriberGuid == null)
                return NotFound();

            Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsRecruiterPolicy");

            if (subscriberGuid == loggedInUserGuid || isAuth.Succeeded)
            {
                SubscriberDto subscriberDto = SubscriberFactory.GetSubscriber(_db, subscriberGuid, _syslog, _mapper);
                return Ok(subscriberDto);
            }
            else
                return Unauthorized();
        }

        [HttpPost("/api/[controller]")]
        public IActionResult NewSubscriber()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Subscriber subscriber = _db.Subscriber.Where(t => t.IsDeleted == 0 && t.SubscriberGuid == subscriberGuid).FirstOrDefault();

            // Subscriber exists do NOT create a duplicate
            if (subscriber != null)
                return BadRequest(new { code = 400, message = "Subscriber is already in the system" });

            subscriber = new Subscriber();
            subscriber.SubscriberGuid = subscriberGuid;
            subscriber.Email = HttpContext.User.FindFirst("emails").Value;
            subscriber.CreateDate = DateTime.UtcNow;
            subscriber.ModifyDate = DateTime.UtcNow;
            subscriber.IsDeleted = 0;
            subscriber.ModifyGuid = Guid.Empty;
            subscriber.CreateGuid = Guid.Empty;

            // Save subscriber to database 
            _db.Subscriber.Add(subscriber);
            _db.SaveChanges();

            return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }

        [HttpPut("/api/[controller]")]
        public IActionResult Update([FromBody] SubscriberDto Subscriber)
        {
            Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (subsriberGuidClaim != Subscriber.SubscriberGuid)
                return Unauthorized();

            Subscriber subscriberFromDb = _db.Subscriber.Where(t => t.IsDeleted == 0 && t.SubscriberGuid == Subscriber.SubscriberGuid).FirstOrDefault();

            var subscriberGuid = new SqlParameter("@SubscriberGuid", Subscriber.SubscriberGuid);
            var firstName = new SqlParameter("@FirstName", (object)(Subscriber.FirstName ?? subscriberFromDb.FirstName) ?? DBNull.Value);
            var lastName = new SqlParameter("@LastName", (object)(Subscriber.LastName ?? subscriberFromDb.LastName) ?? DBNull.Value);
            var address = new SqlParameter("@Address", (object)(Subscriber.Address ?? subscriberFromDb.Address) ?? DBNull.Value);
            var city = new SqlParameter("@City", (object)(Subscriber.City ?? subscriberFromDb.City) ?? DBNull.Value);
            var stateGuid = new SqlParameter("@StateGuid", (Subscriber?.State?.StateGuid != null ? (object)Subscriber.State.StateGuid : (subscriberFromDb.State?.StateGuid != null ? (object)subscriberFromDb.State.StateGuid : DBNull.Value)));
            var phoneNumber = new SqlParameter("@PhoneNumber", (object)(Subscriber.PhoneNumber ?? subscriberFromDb.PhoneNumber) ?? DBNull.Value);
            var facebookUrl = new SqlParameter("@FacebookUrl", (object)Subscriber.FacebookUrl ?? DBNull.Value);
            var twitterUrl = new SqlParameter("@TwitterUrl", (object)Subscriber.TwitterUrl ?? DBNull.Value);
            var linkedInUrl = new SqlParameter("@LinkedInUrl", (object)Subscriber.LinkedInUrl ?? DBNull.Value);
            var stackOverflowUrl = new SqlParameter("@StackOverflowUrl", (object)Subscriber.StackOverflowUrl ?? DBNull.Value);
            var gitHubUrl = new SqlParameter("@GitHubUrl", (object)Subscriber.GithubUrl ?? DBNull.Value);


            DataTable table = new DataTable();
            table.Columns.Add("Guid", typeof(Guid));
            if (Subscriber.Skills != null)
            {
                foreach (var skill in Subscriber.Skills)
                {
                    table.Rows.Add(skill.SkillGuid);
                }
            }

            var skillGuids = new SqlParameter("@SkillGuids", table);
            skillGuids.SqlDbType = SqlDbType.Structured;
            skillGuids.TypeName = "dbo.GuidList";

            var spParams = new object[] { subscriberGuid, firstName, lastName, address, city, stateGuid, phoneNumber, facebookUrl, twitterUrl, linkedInUrl, stackOverflowUrl, gitHubUrl, skillGuids };

            var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Update_Subscriber] 
                    @SubscriberGuid,
                    @FirstName,
	                @LastName,
	                @Address,
	                @City,
	                @StateGuid,
                    @PhoneNumber,
	                @FacebookUrl,
	                @TwitterUrl,
	                @LinkedInUrl,
	                @StackOverflowUrl,
	                @GithubUrl,
	                @SkillGuids", spParams);

            return Ok();
        }

        [HttpPut("/api/[controller]/onboard")]
        public IActionResult Onboard()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                return BadRequest();

            Subscriber subscriber = _db.Subscriber.Where(t => t.IsDeleted == 0 && t.SubscriberGuid == subscriberGuid).FirstOrDefault();

            subscriber.HasOnboarded = 1;
            _db.Subscriber.Update(subscriber);
            _db.SaveChanges();

            return Ok();
        }

        [HttpGet("/api/[controller]/me/group")]
        public async Task<IActionResult> MyGroupsAsync()
        {
            IList<Microsoft.Graph.Group> groups = await _graphClient.GetUserGroupsByObjectId(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            IList<string> response = new List<string>();

            foreach (var group in groups)
            {
                ConfigADGroup acceptedGroup = _configuration.GetSection("ADGroups:Values")
                    .Get<List<ConfigADGroup>>()
                    .Find(e => e.Id == group.Id);

                if (acceptedGroup != null)
                    response.Add(acceptedGroup.Name);
            }

            return Json(new { groups = response });
        }

        [HttpGet("/api/[controller]/search/{searchQuery}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
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
        [Authorize(Policy = "IsRecruiterPolicy")]
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
        }

        // Can we remove subscriberGuid from this route and get it from the user context?
        [HttpGet("/api/[controller]/{subscriberGuid}/skill")]
        public IActionResult GetSkillsBySubscriber(Guid subscriberGuid)
        {
            var subscriberSkills = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid.Value == subscriberGuid)
                .Join(_db.SubscriberSkill.Where(ss => ss.IsDeleted == 0), s => s.SubscriberId, sk => sk.SubscriberId, (s, sk) => new { sk.SkillId })
                .Join(_db.Skill.Where(s => s.IsDeleted == 0), x => x.SkillId, s => s.SkillId, (x, s) => s)
                .Distinct()
                .OrderBy(s => s.SkillName)
                .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
                .ToList();

            return Ok(subscriberSkills);
        }

        [Authorize]
        [HttpGet("/api/[controller]/{subscriberGuid}/file/{fileId}")]
        public async Task<IActionResult> DownloadFile(Guid subscriberGuid, int fileId)
        {
            Guid userGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userGuid != subscriberGuid)
                return Unauthorized();

            Subscriber subscriber = _db.Subscriber.Where(s => s.SubscriberGuid.Equals(subscriberGuid))
                .Include(s => s.SubscriberFile)
                .First();
            SubscriberFile file = subscriber.SubscriberFile.Where(f => f.Id == fileId).First();
            return File(await _cloudStorage.OpenReadAsync(file.BlobName), "application/octet-stream", Path.GetFileName(file.BlobName));
        }

        [Authorize]
        [HttpDelete("/api/[controller]/{subscriberGuid}/file/{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid subscriberGuid, int fileId)
        {
            Guid userGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userGuid != subscriberGuid)
                return Unauthorized();

            Subscriber subscriber = _db.Subscriber.Where(s => s.SubscriberGuid.Equals(subscriberGuid))
                .Include(s => s.SubscriberFile)
                .First();
            SubscriberFile file = subscriber.SubscriberFile.Where(f => f.Id == fileId).First();

            if (!await _cloudStorage.DeleteFileAsync(file.BlobName))
                return BadRequest();

            _db.SubscriberFile.Remove(file);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
