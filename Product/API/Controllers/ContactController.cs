using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore;
using UpDiddyLib.Helpers;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hangfire;
using UpDiddyApi.Workflow;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected readonly ILogger _syslog = null;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ISysEmail _sysemail;
        private readonly IDistributedCache _distributedCache;

        public ContactController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, ILogger<CourseController> syslog, IDistributedCache distributedCache)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _syslog = syslog;
            _httpClientFactory = httpClientFactory;
            _sysemail = sysemail;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(string sort, string name, string email, int? page = null, int? pageSize = 10)
        {
            if (!page.HasValue)
                return Ok(await _db.Contact.ToListAsync());

            var contactQuery = from c in _db.Contact select c;

            if (name != null)
            {
                var names = name.Split(" ");
                if(names.Length == 2)
                {
                    contactQuery = contactQuery.Where(c => c.FirstName.Contains(names[0]) && c.LastName.Contains(names[1]));
                }
                else
                {
                    contactQuery = contactQuery.Where(c => c.FirstName.Contains(names[0]) || c.LastName.Contains(names[0]));
                }
            }

            if (email != null)
                contactQuery = contactQuery.Where(c => c.Email.Contains(email));

            switch (sort)
            {
                case "email asc":
                    contactQuery = contactQuery.OrderBy(c => c.Email);
                    break;
                case "email desc":
                    contactQuery = contactQuery.OrderByDescending(c => c.Email);
                    break;
                case "name asc":
                    contactQuery = contactQuery
                        .OrderBy(c => c.LastName)
                        .ThenBy(c => c.FirstName);
                    break;
                case "name desc":
                    contactQuery = contactQuery
                        .OrderByDescending(c => c.LastName)
                        .ThenByDescending(c => c.FirstName);
                    break;  
            }


            var contacts = await contactQuery.AsNoTracking().Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToListAsync();
            var num_contacts = await contactQuery.CountAsync();
            var num_pages = (num_contacts + pageSize.Value - 1) / pageSize.Value;
            return Ok(new
            {
                totalRecords = num_contacts,
                pages = num_pages,
                data = contacts
            });
        }

        [HttpGet("{contactGuid}")]
        public IActionResult Get(Guid ContactGuid)
        {
            ContactDto rval = null;
            rval = _db.Contact
                .Where(t => t.IsDeleted == 0 && t.ContactGuid == ContactGuid)
                .ProjectTo<ContactDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
            if(rval == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(rval);
            }
        }
    }
}
