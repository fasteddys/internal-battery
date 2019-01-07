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
using UpDiddyApi.Business.Graph;
using UpDiddyApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriberController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private IB2CGraph _graphClient;
        private IConfiguration _configuration;

        public SubscriberController(UpDiddyDbContext db, IB2CGraph client, IConfiguration configuration)
        {
            _db = db;
            _graphClient = client;
            _configuration = configuration;
        }

        [HttpGet("/api/[controller]/me/group")]
        public async Task<IActionResult> MyGroupsAsync()
        {
            IList<Microsoft.Graph.Group> groups = await _graphClient.GetUserGroupsByObjectId(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            IList<string> response = new List<string>();

            foreach(var group in groups)
            {
                ConfigADGroup acceptedGroup = _configuration.GetSection("ADGroups:Values")
                    .Get<List<ConfigADGroup>>()
                    .Find(e => e.Id == group.Id);

                if (acceptedGroup != null)
                    response.Add(acceptedGroup.Name);
            }

            return Json(new { groups = response });
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public IActionResult Get()
        {
            List<Subscriber> subscribers = _db.Subscriber
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .Where(t => t.IsDeleted == 0).ToList<Subscriber>();

            return Json(subscribers);
        }
    }
}
