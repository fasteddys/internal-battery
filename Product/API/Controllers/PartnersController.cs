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
using System.Web;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PartnersController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;
        private IB2CGraph _graphClient;
        private IAuthorizationService _authorizationService;
        private ICloudStorage _cloudStorage;

        public PartnersController(UpDiddyDbContext db,
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {

            Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsCareerCircleAdmin");

            if (isAuth.Succeeded)
            {
                List<Partner> partners = _db.Partner
                    .Where(s => s.IsDeleted == 0)
                    .ToList();
                return Ok(partners);
            }
            else
                return Unauthorized();
        }

        [HttpGet("{PartnerGuid}")]
        public async Task<IActionResult> GetPartner(Guid PartnerGuid)
        {
            Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsCareerCircleAdmin");

            if (isAuth.Succeeded)
            {
                Partner partner = _db.Partner
                    .Where(s => s.IsDeleted == 0 && s.PartnerGuid == PartnerGuid)
                    .FirstOrDefault();
                return Ok(partner);
            }
            else
                return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePartner([FromBody] PartnerDto partnerDto)
        {
            // Ensure required information is present prior to creating new partner
            if(partnerDto == null || string.IsNullOrEmpty(partnerDto.Name) || string.IsNullOrEmpty(partnerDto.Description))
            {
                return BadRequest();
            }

            // Ensure current user is an admin before creating the new partner
            var isAuth = await _authorizationService.AuthorizeAsync(User, "IsCareerCircleAdmin");
            if (isAuth.Succeeded)
            {
                Partner partner = new Partner();
                partner.Name = partnerDto.Name;
                partner.Description = partnerDto.Description;
                partner.PartnerGuid = Guid.NewGuid();
                partner.CreateDate = DateTime.UtcNow;
                partner.ModifyDate = DateTime.UtcNow;
                partner.IsDeleted = 0;
                partner.ModifyGuid = Guid.Empty;
                partner.CreateGuid = Guid.Empty;
                _db.Partner.Add(partner);
                _db.SaveChanges();
                return Created(_configuration["Environment:ApiUrl"] + "partners/" + partner.PartnerGuid, partnerDto);
                return Ok();
            }
            else
                return Unauthorized();
        }
    }
}
