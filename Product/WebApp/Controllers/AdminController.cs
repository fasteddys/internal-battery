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
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

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
        [Route("/admin/contact")]
        public IActionResult Contact()
        {
            return View();
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

        [HttpGet]
        [Route("/admin/partners/contacts/{PartnerGuid}")]
        public async Task<IActionResult> ContactsAsync(Guid PartnerGuid)
        {
            try
            {
                PartnerDto partner = await _api.GetPartnerAsync(PartnerGuid);

                if (partner == null)
                    return NotFound();


                return View("Contacts", partner);
            }
            catch(ApiException e)
            {
                return BadRequest();
            }
            
            
        }

        [HttpPut]
        [Route("admin/contacts/import/{partnerGuid}/{cacheKey}")]
        public IActionResult ImportContacts(Guid partnerGuid, string cacheKey)
        {
            return new JsonResult(_api.ImportContactsAsync(partnerGuid, cacheKey));
        }

        [HttpPost]
        public IActionResult UploadContacts(IFormFile contactsFile)
        {
            ImportValidationSummaryDto importValidationSummary = new ImportValidationSummaryDto();

            var contacts = LoadContactsFromCsv(contactsFile);
            if (contacts != null)
            {
                // perform basic validation on the contacts loaded from the csv file 
                importValidationSummary.ImportActions = PerformBasicValidationOnContacts(ref contacts);
                // load a handful of contact records for the UI preview
                importValidationSummary.ContactsPreview = contacts.Take(5).ToList();
                // store all contact data to be imported in redis (after removing items that will be skipped)
                string cacheKey = StashContactForImportInRedis(contacts, Guid.Parse(this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value));
                // return the cache key to the UI so that the data can be retrieved later if processing continues
                importValidationSummary.CacheKey = cacheKey;
            }
            else
            {
                // no contacts could be loaded; indicate this in the import action
                importValidationSummary.ImportActions.Add(new ImportActionDto()
                {
                    Count = 0,
                    ImportBehavior = ImportBehavior.Pending,
                    Reason = "No records could be loaded from the selected file(s)"
                });
            }

            return new JsonResult(importValidationSummary);
        }

        /// <summary>
        /// Performs basic validation on the list of contacts uploaded (required fields, duplicates by email)
        /// </summary>
        /// <param name="contacts">The list of contacts to be acted upon; note that this is passed by referenced and will be modified if any contacts fail validation</param>
        /// <returns></returns>
        private List<ImportActionDto> PerformBasicValidationOnContacts(ref List<ContactDto> contacts)
        {
            List<ImportActionDto> importActions = new List<ImportActionDto>();
            int contactsToBeProcessed = contacts.Count();

            // email check
            var missingEmail = contacts.Where(c => string.IsNullOrWhiteSpace(c.Email)).ToList();
            if (missingEmail != null && missingEmail.Count() > 1)
            {
                importActions.Add(
                    new ImportActionDto()
                    {
                        Count = missingEmail.Count(),
                        ImportBehavior = ImportBehavior.Ignored,
                        Reason = "missing required field: Email"
                    });
                contactsToBeProcessed -= contacts.RemoveAll(c => string.IsNullOrWhiteSpace(c.Email));
            }

            // metadata check
            var missingMetadataRequiredFields = contacts.Where(c => !c.Metadata.ContainsKey("FirstName") || c.Metadata.ContainsKeyValue("FirstName", string.Empty) || !c.Metadata.ContainsKey("LastName") || c.Metadata.ContainsKeyValue("LastName", string.Empty)).ToList();
            if (missingMetadataRequiredFields != null && missingMetadataRequiredFields.Count() > 1)
            {
                importActions.Add(
                    new ImportActionDto()
                    {
                        Count = missingMetadataRequiredFields.Count(),
                        ImportBehavior = ImportBehavior.Ignored,
                        Reason = "missing required field(s): FirstName, LastName"
                    });
                contactsToBeProcessed -= contacts.RemoveAll(c => !c.Metadata.ContainsKey("FirstName") || !c.Metadata.ContainsKey("LastName"));
            }

            // duplicate check
            var duplicates = (from c in contacts
                              group c by new { c.Email } into g
                              where g.Count() > 1
                              select new
                              {
                                  Email = g.First().Email,
                                  Instances = g.Count(),
                                  SourceSystemIdentifier = g.First().SourceSystemIdentifier,
                                  Metadata = g.First().Metadata
                              }).ToList();
            if (duplicates != null && duplicates.Count() > 1)
            {
                importActions.Add(
                    new ImportActionDto()
                    {
                        Count = duplicates.Sum(d => d.Instances) - duplicates.Count,
                        ImportBehavior = ImportBehavior.Ignored,
                        Reason = "identified as duplicates"
                    });
                contactsToBeProcessed -= contacts.RemoveAll(c => duplicates.Select(d => d.Email).Contains(c.Email));
            }

            // remaining records to be processed
            importActions.Add(
                new ImportActionDto()
                {
                    Count = contactsToBeProcessed,
                    ImportBehavior = ImportBehavior.Pending,
                    Reason = "passed validation rules"
                });

            return importActions;
        }

        private List<ContactDto> LoadContactsFromCsv(IFormFile contactsFile)
        {
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
                }
            }
            return contacts;
        }

        private string StashContactForImportInRedis(List<ContactDto> contacts, Guid subscriberGuid)
        {
            int cacheTtl = int.Parse(_configuration["redis:cacheTTLInMinutes"]);
            string contactsJson = Newtonsoft.Json.JsonConvert.SerializeObject(contacts);
            string cacheKey = $"contactImport:{Guid.NewGuid()}";
            _cache.SetString(cacheKey, contactsJson, new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTtl) });
            return cacheKey;
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

        [Authorize]
        [HttpGet]
        public IActionResult Partners()
        {
            return View();
        }

        [Authorize]
        [HttpGet("/admin/modifypartner/{PartnerGuid}")]
        public async Task<IActionResult> ModifyPartnerAsync(Guid PartnerGuid)
        {
            PartnerDto partner = await _api.GetPartnerAsync(PartnerGuid);

            if (partner == null)
                return NotFound();

            return View("ModifyPartner", partner);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePartnerAsync(PartnersViewModel UpdatedPartner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    BasicResponseDto NewPartnerResponse = await _api.UpdatePartnerAsync(new PartnerDto
                    {
                        PartnerGuid = UpdatedPartner.PartnerGuid,
                        Name = UpdatedPartner.Name,
                        Description = UpdatedPartner.Description
                    });
                    return RedirectToAction("Partners");
                }
                catch (ApiException e)
                {
                    // Log exception
                }
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult> PartnerGridAsync(String searchQuery)
        {
            IList<PartnerDto> partners = await _api.GetPartnersAsync();
            return PartialView("Admin/_PartnerGrid", partners);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddPartner()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePartnerAsync(PartnersViewModel NewPartner)
        {


            if (ModelState.IsValid)
            {
                try
                {
                    PartnerDto newPartnerFromDb = await _api.CreatePartnerAsync(new PartnerDto
                    {
                        PartnerGuid = NewPartner.PartnerGuid,
                        Name = NewPartner.Name,
                        Description = NewPartner.Description
                    });
                    return RedirectToAction("Partners");
                }
                catch(ApiException e)
                {
                    // Log error
                }                
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet("/admin/deletepartner/{PartnerGuid}")]
        public async Task<IActionResult> DeleteParterAsync(Guid PartnerGuid)
        {
            try
            {
                BasicResponseDto response = await _api.DeletePartnerAsync(PartnerGuid);
                if (response.StatusCode != 200)
                    return BadRequest();
                return RedirectToAction("Partners");
            }
            catch(ApiException e)
            {
                // Log error
            }
            return BadRequest();
        }
    }
}