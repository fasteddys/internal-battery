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
    [Route("/V2/[controller]/")]
    public class TagsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHangfireService _hangfireService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ITagService _tagservice;

        public TagsController(IMapper mapper
        , IConfiguration configuration
        , IHangfireService hangfireService
        , ITagService tagService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _tagservice = tagService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTags(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var tags = await _tagservice.GetTags(limit, offset, sort, order);
            return Ok(tags);
        }

        [HttpGet]
        [Route("keyword")]
        public async Task<IActionResult> GetTagsByKeyword([FromQuery] string value)
        {
            var tags = await _tagservice.GetTagsByKeyword(value);
            return Ok(tags);
        }

        [HttpGet]
        [Route("{tag:guid}")]
        public async Task<IActionResult> GetTag(Guid tag)
        {
            var result = await _tagservice.GetTag(tag);
            return Ok(result);
        }

        [HttpPut]
        [Route("{tag:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateTag(Guid tag, [FromBody] TagDto tagDto)
        {
            await _tagservice.UpdateTag(tag, tagDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{tag:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteTag(Guid tag)
        {
            await _tagservice.DeleteTag(tag);
            return StatusCode(204);
        }

        [HttpPost]
        [Authorize(Policy = "IsRecruiterOrAdmin")]
        public async Task<IActionResult> CreateTag(Guid tag, [FromBody] TagDto tagDto)
        {
            var tagGuid = await _tagservice.CreateTag(tagDto);
            return StatusCode(201, tagGuid);
        }
    }
}