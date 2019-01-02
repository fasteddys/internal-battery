using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class SubscriberController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        public SubscriberController(UpDiddyDbContext db)
        {
            _db = db;
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get()
        {
            List<Subscriber> subscribers = _db.Subscriber
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .Where(t => t.IsDeleted == 0).ToList<Subscriber>();

            return Json(subscribers);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
    }
}
