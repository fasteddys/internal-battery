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
    public class AdminController : Controller
    {
        private IApi _api;

        public AdminController(IApi api)
        {
            _api = api;
        }

        [HttpGet]
        [Route("/admin/courselookup")]
        public JsonResult CourseLookup()
        {
            var selectListCourses =
                _api.Courses()
                .Select(course => new
                {
                    entityGuid = course.CourseGuid,
                    entityName = course.Name
                })
                .OrderBy(e => e.entityName)
                .ToList();

            return new JsonResult(selectListCourses);
        }

        [HttpGet]
        [Route("/admin/subscriberlookup")]
        public JsonResult SubscriberLookup()
        {
            var selectListSubscribers =
                _api.SubscriberSearch(string.Empty)
                    .Select(subscriber => new
                    {
                        entityGuid = subscriber.SubscriberGuid,
                        entityName = subscriber.Email
                    })
                    .OrderBy(e => e.entityName)
                    .ToList();

            return new JsonResult(selectListSubscribers);
        }

        [HttpGet]
        [Route("/admin/skillslookup/{entityType}/{entityGuid}")]
        public JsonResult SkillsLookup(string entityType, Guid entityGuid)
        {
            var selectListSkills =
                _api.GetEntitySkills(entityType, entityGuid)
                .Select(skill => new
                {
                    skillGuid = skill.SkillGuid,
                    skillName = skill.SkillName
                })
                .OrderBy(s => s.skillName)
                .ToList();

            return new JsonResult(selectListSkills);
        }

        [HttpGet]
        [Route("/admin/skills")]
        public IActionResult Skills()
        {
            return View();
        }

        [HttpPut]
        [Route("/admin/skills")]
        public IActionResult UpdateSkills([FromBody] EntitySkillDto entitySkillDto)
        {
            // todo: exception handling
            _api.UpdateEntitySkills(entitySkillDto);
            return Ok();
        }
    }
}