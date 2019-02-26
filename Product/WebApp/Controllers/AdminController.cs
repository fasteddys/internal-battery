using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    [Authorize(Policy = "IsCareerCircleAdmin")]
    public class AdminController : Controller
    {
        private IApi _api;
        private IConfiguration _configuration;
        private IDistributedCache _cache;

        public AdminController(IApi api, IConfiguration configuration, IDistributedCache cache)
        {
            _api = api;
            _configuration = configuration;
            _cache = cache;
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
            ImportValidationSummary importValidationSummary = new ImportValidationSummary();
            List<ContactDto> contacts;
            using (var reader = new StreamReader(contactsFile.OpenReadStream()))
            {
                using (var csv = new CsvReader(reader))
                {
                    // do this outside of ContactDtoMap for performance reasons
                    List<string> declaredProperties = typeof(ContactDto)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Select(p => p.Name.ToLower())
                        .ToList();

                    ContactDtoMap contactDtoMap = new ContactDtoMap(declaredProperties);
                    csv.Configuration.RegisterClassMap(contactDtoMap);
                    csv.Configuration.Delimiter = ",";
                    csv.Configuration.PrepareHeaderForMatch = (string header, int index) =>
                    {
                        var newHeader = Regex.Replace(header, @"\s", string.Empty);
                        newHeader = newHeader.Trim();
                        newHeader = newHeader.ToLower();
                        return newHeader;
                    };

                    contacts = csv.GetRecords<ContactDto>().ToList();

                    // sample validation message - remove this
                    importValidationSummary.ImportActions.Add(new ImportAction()
                    {
                        Count = contacts.Count,
                        ImportBehavior = ImportBehavior.Created,
                        Reason = "because they do not yet exist for {Partner}"
                    });

                    /* todo: perform all validation (including server-side)
                     * example of what the response should look like:
                     * 
                     * 2 rows will be skipped because the email appears more than once in this list
                     * 7 rows will be skipped because they are missing required fields
                     * 14 rows will be skipped because they contain invalid data and could not be parsed
                     * 22 existing contact records for {Partner} will be updated
                     * 2932 new contact records for {Partner} will be created
                     */

                    if (contacts != null)
                    {
                        // load a handful of contact records for the UI preview
                        importValidationSummary.ContactsPreview = contacts.Take(5).ToList();
                        // store all contact data to be imported in redis (after removing items that will be skipped)
                        string cacheKey = StashContactForImportInRedis(contacts, Guid.Parse(this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value), contactsFile.FileName);
                        // return the cache key to the UI so that the data can be retrieved later if processing continues
                        importValidationSummary.CacheKey = cacheKey;
                    }
                }
            }

            return new JsonResult(importValidationSummary);
        }

        private string StashContactForImportInRedis(List<ContactDto> contacts, Guid subscriberGuid, string fileName)
        {
            int cacheTtl = int.Parse(_configuration["redis:cacheTTLInMinutes"]);
            string contactsJson = Newtonsoft.Json.JsonConvert.SerializeObject(contacts);
            string cacheKey = subscriberGuid.ToString() + ":" + fileName + ":" + DateTime.UtcNow.ToLongTimeString();
            _cache.SetString(cacheKey, contactsJson, new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTtl) });
            return cacheKey;
        }

        public class ImportValidationSummary
        {
            public string CacheKey { get; set; }
            public List<ImportAction> ImportActions { get; set; } = new List<ImportAction>();
            public List<ContactDto> ContactsPreview { get; set; } = new List<ContactDto>();
        }

        public class ImportAction
        {
            // todo: create constructor that encapsulates logic for presenting this information?
            // e.g. formatted message combining the import behavior, count of records affected, and reason (which may include the partner name)
            public ImportBehavior ImportBehavior { get; set; }
            public string Reason { get; set; }
            public int Count { get; set; }
        }

        public enum ImportBehavior
        {
            Ignored = 0,
            Created = 1,
            Updated = 2
        }

        public sealed class ContactDtoMap : ClassMap<ContactDto>
        {
            public ContactDtoMap(List<string> declaredProperties)
            {
                Map(c => c.Email).Name("email");
                Map(c => c.SourceSystemIdentifier).Name("sourcesystemidentifier");
                Map(c => c.Metadata).ConvertUsing(row =>
                {
                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                    var metadataColumnNames = row.Context.HeaderRecord.ToList().Except(declaredProperties).ToList();
                    if (metadataColumnNames != null && metadataColumnNames.Count > 0)
                    {
                        foreach (var metaDataColumnName in metadataColumnNames)
                        {
                            string fieldValue = null;
                            if (row.TryGetField<string>(metaDataColumnName, out fieldValue))
                            {
                                metadata.Add(metaDataColumnName, fieldValue);
                            }
                        }
                    }

                    return metadata;
                });
            }
        }
    }
}