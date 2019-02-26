using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [HttpGet]
        [Route("/admin/contacts")]
        public IActionResult Contacts()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [Route("/admin/uploadcontacts")]
        public IActionResult UploadContacts(IFormFile contactsFile)
        {
            IEnumerable<ContactDto> contacts;
            using (var reader = new StreamReader(contactsFile.OpenReadStream()))
            {
                using (var csv = new CsvReader(reader))
                {
                    // use this? csv.Configuration.PrepareHeaderForMatch
                    csv.Configuration.RegisterClassMap<ContactDtoMap>();
                    contacts = csv.GetRecords<ContactDto>();
                }
            }

            if (contacts != null)
            {
                // validation
            }

            return new JsonResult(contacts);
        }
        
        public sealed class ContactDtoMap : ClassMap<ContactDto>
        {
            public ContactDtoMap()
            {
                Map(c => c.Email).Name("email");
                Map(c => c.FirstName).Name("first_name");
                Map(c => c.LastName).Name("last_name");
                Map(c => c.ContactGuid).Ignore();
                Map(c => c.CreateDate).Ignore();
                Map(c => c.CreateGuid).Ignore();
                Map(c => c.IsDeleted).Ignore();
                Map(c => c.ModifyDate).Ignore();
                Map(c => c.ModifyGuid).Ignore();
            }
        }
    }
}