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
            //var testing = _api.UpdateEntitySkills(new UpDiddyLib.Dto.EntitySkillDto()
            //{
            //    EntityType = "Course",
            //    EntityGuid = Guid.Parse("9C4E9CE7-EB4D-43A8-9D55-8438769C285D"),
            //    Skills = new List<SkillDto> {
            //        new SkillDto() { SkillGuid = Guid.Parse("E4A11336-6C37-42A7-A486-C708E1F15410")},
            //        new SkillDto() { SkillGuid = Guid.Parse("5EA4EC82-21BD-4B32-99B0-00C585BC005A")},
            //        new SkillDto() { SkillGuid = Guid.Parse("A7D33E93-5346-4EEC-B909-366A9330F26D")}
            //    }
            //});

            return View();
        }
    }
}