using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
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
        public async Task<IActionResult> GetSkills(int limit, int offset, string sort, string order)
        {
            var skills = await _skillservice.GetSkills(limit, offset, sort, order);
            return Ok(skills);
        }

        [HttpGet]
        [Route("/V2/[controller]/{skill}")]
        public async Task<IActionResult> GetSkill(Guid skill)
        {
            var result = await _skillservice.GetSkill(skill);
            return Ok(result);
        }

        [HttpPut]
        [Route("/V2/[controller]/{skill}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateSkill(Guid skill, SkillDto skillDto)
        {
            await _skillservice.UpdateSkill(skill, skillDto);
            return StatusCode(200);
        }

        [HttpDelete]
        [Route("/V2/[controller]/{skill}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteSkill(Guid skill, SkillDto skillDto)
        {
            await _skillservice.UpdateSkill(skill, skillDto);
            return StatusCode(204);
        }

        [HttpPost]
        [Route("/V2/[controller]/{skill}")]
        [Authorize]
        public async Task<IActionResult> CreateSkill(Guid skill, SkillDto skillDto)
        {
            await _skillservice.CreateSkill(skillDto);
            return StatusCode(201);
        }
    }
}