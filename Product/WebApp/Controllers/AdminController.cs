using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddy.Api;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    [Authorize(Policy = "IsCareerCircleAdmin")]
    public class AdminController : BaseController
    {
        private IApi _api;

        public AdminController(IApi api) : base(api)
        {
            _api = api;
        }

        [HttpGet]
        [Route("/admin/courselookup")]
        public async Task<JsonResult> CourseLookup()
        {
            var selectListCourses = await _api.CoursesAsync();

            var list = selectListCourses
                .Select(course => new
                {
                    entityGuid = course.CourseGuid,
                    entityName = course.Name
                })
                .OrderBy(e => e.entityName)
                .ToList();

            return new JsonResult(list);
        }

        [HttpGet]
        [Route("/admin/subscriberlookup")]
        public async Task<JsonResult> SubscriberLookup()
        {
            IList<SubscriberDto> subs = await _api.SubscriberSearchAsync(string.Empty);

            var list = subs
                .Select(subscriber => new
                {
                    entityGuid = subscriber.SubscriberGuid,
                    entityName = subscriber.Email
                })
                .OrderBy(e => e.entityName)
                .ToList();

            return new JsonResult(list);
        }

        [HttpGet]
        [Route("/admin/skillslookup/{entityType}/{entityGuid}")]
        public async Task<JsonResult> SkillsLookup(string entityType, Guid entityGuid)
        {
            var selectListSkills = await _api.GetEntitySkillsAsync(entityType, entityGuid);

            var list = selectListSkills
                .Select(skill => new
                {
                    skillGuid = skill.SkillGuid,
                    skillName = skill.SkillName
                })
                .OrderBy(s => s.skillName)
                .ToList();

            return new JsonResult(list);
        }

        [HttpGet]
        [Route("/admin/skills")]
        public IActionResult Skills()
        {
            return View();
        }

        [HttpPut]
        [Route("/admin/skills")]
        public async Task<IActionResult> UpdateSkills([FromBody] EntitySkillDto entitySkillDto)
        {
            // todo: exception handling
            await _api.UpdateEntitySkillsAsync(entitySkillDto);
            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("/admin/partners")]
        public async Task<IActionResult> Partners()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult> PartnerGrid(String searchQuery)
        {
            IList<PartnerDto> partners = await _api.GetPartnersAsync();
            return PartialView("_PartnerGrid", partners);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddPartner()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePartnerAsync(PartnerDto NewPartner)
        {
            PartnerDto newPartnerFromDb = await _api.CreatePartnerAsync(NewPartner);
            return RedirectToAction("Partners");
        }
    }
}