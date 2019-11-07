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

using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHangfireService _hangfireService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISkillService _skillservice;


        public SkillsController(IMapper mapper
        , IConfiguration configuration
        , IHangfireService hangfireService
        , ISkillService skillService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _skillservice = skillService;
        }

        [HttpGet]
        [Route("/V2/[controller]/")]
        [Authorize]
        public async Task<IActionResult> GetSkills()
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var skills = await _skillservice.GetSkillsBySubscriberGuid(subscriberGuid);
            return Ok(skills);             
        }

        [HttpPut]
        [Route("/V2/[controller]/")]
        [Authorize]
        public async Task<IActionResult> UpdateSubscriberSkills([FromBody] List<string> skills)
        {
             Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
             await _skillservice.UpdateSubscriberSkills(subscriberGuid, skills);
             return StatusCode(201);
        }
    }
}