using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Http;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using Skill = UpDiddyApi.Models.Skill;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class CourseService : ICourseService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private IHangfireService _hangfireService;
        private ISysEmail _sysEmail;
        private readonly IServiceProvider _services;
        public CourseService(IServiceProvider services, IRepositoryWrapper repositoryWrapper, IMapper mapper, IHangfireService hangfireService, IConfiguration configuration)
        {
            _services = services;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = configuration;
            _hangfireService = hangfireService;
            _sysEmail = _services.GetService<ISysEmail>();
        }



        #region Course Crud

        public async Task<List<RelatedCourseDto>> GetCoursesByCourses(List<Guid> courseGuids, int limit, int offset)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetCoursesByCourses(courseGuids, limit, offset);
        }


        public async Task<List<RelatedCourseDto>> GetCoursesByCourse(Guid courseGuid, int limit, int offset)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetCoursesByCourse(courseGuid, limit, offset);
        }

        public async Task<List<RelatedCourseDto>> GetCoursesBySubscriber(Guid subscriberGuid, int limit, int offset)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetCoursesBySubscriber(subscriberGuid, limit, offset);
        }
        
        public async Task<List<RelatedCourseDto>> GetCoursesByJobs(List<Guid> jobPostingGuids, int limit, int offset)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetCoursesByJobs(jobPostingGuids, limit, offset);
        }

        public async Task<List<RelatedCourseDto>> GetCoursesByJob(Guid jobPostingGuid, int limit, int offset)
        {
            return await _repositoryWrapper.StoredProcedureRepository.GetCoursesByJob(jobPostingGuid, limit, offset);
        }


        public async Task<List<CourseDetailDto>> GetCoursesRandom(IQueryCollection query)
        {

            int MaxResults = 3;
            int.TryParse(_config["APIGateway:DefaultMaxLimit"], out MaxResults);
            string limit = query["limit"];
            if (limit != null)
                int.TryParse(limit, out MaxResults);
            var courses = await _repositoryWrapper.StoredProcedureRepository.GetCoursesRandom(MaxResults);
            return courses;
        }


        public async Task<List<CourseDetailDto>> GetCourses(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var courses = await _repositoryWrapper.StoredProcedureRepository.GetCourses(limit, offset, sort, order);
            if (courses == null)
                throw new NotFoundException("Courses not found");
            return (courses);
        }

        public async Task<CourseDetailDto> GetCourse(Guid courseGuid)
        {
            if (courseGuid == null || courseGuid == Guid.Empty)
                throw new NullReferenceException("CourseGuid cannot be null");
            var course = await _repositoryWrapper.StoredProcedureRepository.GetCourse(courseGuid);
            if (course == null)
                throw new NotFoundException("Courses not found");
            return (course);
        }

        public async Task<int> GetCoursesCount()
        {
            return await _repositoryWrapper.Course.GetCoursesCount();
        }
        
        /// <summary>
        /// Handles the creation of a course. 
        /// Note that this only supports a single course variant at this time and that several properties are unsupported because they are not being used.
        /// </summary>
        /// <param name="courseDto"></param>
        /// <returns></returns>
        public async Task<int> AddCourseAsync(CourseDto courseDto)
        {
            // lookup vendor, create it if it dooes not exist, and retrieve the identifier to use for the course
            var vendorGuid = await GetVendorGuidAsync(courseDto.Vendor);

            // process tag topics and retrieve list of tags to associate with the course
            var tagGuids = await ProcessTagTopicsAsync(courseDto.TagTopics);

            // lookup skills, create those that do not exist, and retrieve list of skills to associate with the course
            var skillGuids = await GetSkillGuidsAsync(courseDto.Skills);

            // lookup course variant type - we only need to support one course variant for now
            var courseVariant = courseDto.CourseVariants.FirstOrDefault();
            var courseVariantTypeGuid = await GetCourseVariantTypeGuidAsync(courseVariant.CourseVariantType);

            // create the course and return the id
            return await _repositoryWrapper.StoredProcedureRepository.AddOrUpdateCourseAsync(new CourseParams()
            {
                CourseId = courseDto.CourseId == 0 ? null : (int?)courseDto.CourseId,
                VendorGuid = vendorGuid,
                CourseVariantTypeGuid = courseVariantTypeGuid,
                Code = courseDto.Code,
                Description = courseDto.Description,
                IsExternal = courseDto.IsExternal,
                Name = courseDto.Name,
                Price = courseVariant.Price,
                SkillGuids = skillGuids,
                TagGuids = tagGuids
            });
        }

        /// <summary>
        /// Handles the logical deletion of a course. 
        /// </summary>
        /// <param name="courseDto"></param>
        /// <returns></returns>
        public async Task DeleteCourseAsync(Guid courseGuid)
        {
            var courseLookup = await _repositoryWrapper.Course.GetByConditionAsync(c => c.CourseGuid == courseGuid);
            var course = courseLookup.FirstOrDefault();
            course.IsDeleted = 1;
            course.ModifyDate = DateTime.UtcNow;
            course.ModifyGuid = Guid.Empty;
            _repositoryWrapper.Course.Update(course);
            await _repositoryWrapper.Course.SaveAsync();
        }

        /// <summary>
        /// Handles the update of a course. 
        /// Note that this only supports a single course variant at this time and that several properties are unsupported because they are not being used.
        /// </summary>
        /// <param name="courseDto"></param>
        /// <returns></returns>
        public async Task<int> EditCourseAsync(CourseDto courseDto)
        {
            // lookup vendor, create it if it dooes not exist, and retrieve the identifier to use for the course
            var vendorGuid = await GetVendorGuidAsync(courseDto.Vendor);

            // process tag topics and retrieve list of tags to associate with the course
            var tagGuids = await ProcessTagTopicsAsync(courseDto.TagTopics);

            // lookup skills, create those that do not exist, and retrieve list of skills to associate with the course
            var skillGuids = await GetSkillGuidsAsync(courseDto.Skills);

            // lookup course variant type - we only need to support one course variant for now
            var courseVariant = courseDto.CourseVariants.FirstOrDefault();
            var courseVariantTypeGuid = await GetCourseVariantTypeGuidAsync(courseVariant.CourseVariantType);

            // create the course and return the id
            return await _repositoryWrapper.StoredProcedureRepository.AddOrUpdateCourseAsync(new CourseParams()
            {
                CourseId = courseDto.CourseId == 0 ? null : (int?)courseDto.CourseId,
                VendorGuid = vendorGuid,
                CourseVariantTypeGuid = courseVariantTypeGuid,
                Code = courseDto.Code,
                Description = courseDto.Description,
                IsExternal = courseDto.IsExternal,
                Name = courseDto.Name,
                Price = courseVariant.Price,
                SkillGuids = skillGuids,
                TagGuids = tagGuids
            });
        }


        #endregion

        #region Course Variants 

        public async Task<List<CourseVariantDetailDto>> GetCourseVariants(Guid courseGuid)
        {
            if (courseGuid == null || courseGuid == Guid.Empty)
                throw new NullReferenceException("CourseGuid cannot be null");
            var variants = await _repositoryWrapper.StoredProcedureRepository.GetCourseVariants(courseGuid);
            if (variants == null)
                throw new NotFoundException("Courses not found");
            return (variants);
        }



        #endregion

        #region Course Search 
 
        public async Task<CourseSearchResult> SearchCoursesAsync(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", string level = "", string topic = "" )
        { 
            DateTime startSearch = DateTime.Now;
            CourseSearchResult searchResults = new CourseSearchResult();
     
            string searchServiceName =  _config["AzureSearch:SearchServiceName"];
            string adminApiKey = _config["AzureSearch:SearchServiceQueryApiKey"];
            string courseIndexName = _config["AzureSearch:CourseIndexName"];

            // map descending to azure search sort syntax of "asc" or "desc"  default is ascending so only map descending 
            string orderBy = sort;
            if (order == "descending")
                orderBy =  orderBy + " desc";
            List<String> orderByList = new List<string>();
            orderByList.Add(orderBy);

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

            // Create an index named hotels
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(courseIndexName);

            SearchParameters parameters;
            DocumentSearchResult<CourseDetailDto> results;
 
            parameters =
                new SearchParameters()
                {
                   Top = limit,
                   Skip = offset,
                   OrderBy = orderByList,
                   IncludeTotalResultCount = true ,                 
                };


            if ( level != "" )
                parameters.Filter = $"Level eq '{level}'";

            if (string.IsNullOrEmpty(parameters.Filter) == false && topic != "")
                parameters.Filter += " and  ";

            if ( topic != "")
                parameters.Filter += $"Topic eq '{topic}'";

 
            results = indexClient.Documents.Search<CourseDetailDto>(keyword, parameters);

            DateTime startMap = DateTime.Now;
            searchResults.Courses = results?.Results?
                .Select(s => (CourseDetailDto) s.Document)
                .ToList();

            searchResults.TotalHits = results.Count.Value;
            searchResults.PageSize = limit;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.CourseCount = searchResults.Courses.Count;
            searchResults.PageNum = (offset / limit) + 1;
 
            DateTime stopMap = DateTime.Now;

            // calculate search timing metrics 
            TimeSpan intervalTotalSearch = stopMap - startSearch;
            TimeSpan intervalSearchTime = startMap - startSearch;
            TimeSpan intervalMapTime = stopMap - startMap;

            // assign search metrics to search results 
            searchResults.SearchTimeInMilliseconds = intervalTotalSearch.TotalMilliseconds;
            searchResults.SearchQueryTimeInTicks = intervalSearchTime.Ticks;
            searchResults.SearchMappingTimeInTicks = intervalMapTime.Ticks;
            return searchResults;
        }



            #endregion

        #region Private Members

            /// <summary>
            /// Handles the lookup and creation of the course variant type to be associated with the course being created/updated.
            /// </summary>
            /// <param name="vendorDto"></param>
            /// <returns>An identifier for the course variant type to be associated with the course being created/updated.</returns>
            private async Task<Guid> GetCourseVariantTypeGuidAsync(CourseVariantTypeDto courseVariantTypeDto)
        {
            Guid courseVariantTypeGuid = Guid.Empty;
            var courseVariantTypeLookup = await _repositoryWrapper.CourseVariantType.GetByConditionAsync(cvt => cvt.Name == courseVariantTypeDto.Name);
            var courseVariantType = courseVariantTypeLookup.FirstOrDefault();
            if (courseVariantType == null)
            {
                await _repositoryWrapper.CourseVariantType.Create(new CourseVariantType()
                {
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0,
                    Name = courseVariantType.Name,
                    CourseVariantGuid = Guid.NewGuid()
                });
                await _repositoryWrapper.CourseVariantType.SaveAsync();
                var newCourseVariantTypeLookup = await _repositoryWrapper.CourseVariantType.GetByConditionAsync(cvt => cvt.Name == courseVariantTypeDto.Name);
                courseVariantType = newCourseVariantTypeLookup.FirstOrDefault();
            }
            if (courseVariantType.CourseVariantGuid.HasValue)
                courseVariantTypeGuid = courseVariantType.CourseVariantGuid.Value;
            return courseVariantTypeGuid;
        }

        /// <summary>
        /// Handles the lookup and creation of the vendor to be associated with the course being created/updated.
        /// </summary>
        /// <param name="vendorDto"></param>
        /// <returns>An identifier for the vendor to be associated with the course being created/updated.</returns>
        private async Task<Guid> GetVendorGuidAsync(VendorDto vendorDto)
        {
            Guid vendorGuid = Guid.Empty;
            var vendorLookup = await _repositoryWrapper.Vendor.GetByConditionAsync(v => v.Name == vendorDto.Name);
            var vendor = vendorLookup.FirstOrDefault();
            if (vendor == null)
            {
                await _repositoryWrapper.Vendor.Create(new Vendor()
                {
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0,
                    LoginUrl = null,
                    Name = vendorDto.Name,
                    VendorGuid = Guid.NewGuid()
                });
                await _repositoryWrapper.Vendor.SaveAsync();
                var newVendorLookup = await _repositoryWrapper.Vendor.GetByConditionAsync(v => v.Name == vendorDto.Name);
                vendor = newVendorLookup.FirstOrDefault();
            }
            if (vendor.VendorGuid.HasValue)
                vendorGuid = vendor.VendorGuid.Value;
            return vendorGuid;
        }

        /// <summary>
        /// Handles the lookup and creation of skills to be associated with the course being created/updated.
        /// Duplicate skills are not included in the result.
        /// </summary>
        /// <param name="skills"></param>
        /// <returns>A list of skill identifiers to be associated with the course being created/updated.</returns>
        private async Task<List<Guid>> GetSkillGuidsAsync(List<UpDiddyLib.Dto.SkillDto> skills)
        {
            List<Guid> skillGuids = new List<Guid>();
            // lookup skills
            foreach (var skillDto in skills)
            {
                Skill skill = null;
                var skillLookup = await _repositoryWrapper.SkillRepository.GetByConditionAsync(s => s.SkillName == skillDto.SkillName.Trim().ToLower());
                skill = skillLookup.FirstOrDefault();
                if (skill == null)
                {
                    // create the skill if it does not exist
                    await _repositoryWrapper.SkillRepository.Create(new Skill()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        SkillGuid = Guid.NewGuid(),
                        SkillName = skillDto.SkillName.Trim().ToLower()
                    });
                    await _repositoryWrapper.SkillRepository.SaveAsync();
                    var newSkillLookup = await _repositoryWrapper.SkillRepository.GetByConditionAsync(s => s.SkillName == skillDto.SkillName);
                    skill = newSkillLookup.FirstOrDefault();
                }
                // add to list of skills to be associated with course
                if (skill.SkillGuid.HasValue && !skillGuids.Contains(skill.SkillGuid.Value))
                    skillGuids.Add(skill.SkillGuid.Value);
            }
            return skillGuids;
        }

        /// <summary>
        /// Handles the creation of tags, topics, and tag topics. Duplicates tags are not included in the result.
        /// This process does belong here as opposed to some other service because the implementation of tags and topics is specific to courses.
        /// </summary>
        /// <param name="tagTopics"></param>
        /// <returns>A list of tag identifiers to be associated with the course being created/updated.</returns>
        private async Task<List<Guid>> ProcessTagTopicsAsync(List<TagTopicDto> tagTopics)
        {
            List<Guid> tagGuids = new List<Guid>();

            foreach (var tagTopicDto in tagTopics)
            {
                // lookup tag (and create if necessary)
                var tagLookup = await _repositoryWrapper.Tag.GetByConditionAsync(t => t.Name == tagTopicDto.Tag.Name);
                var tag = tagLookup.FirstOrDefault();
                if (tag == null)
                {
                    await _repositoryWrapper.Tag.Create(new Tag()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        Description = tagTopicDto.Tag.Description,
                        IsDeleted = 0,
                        Name = tagTopicDto.Tag.Name,
                        TagGuid = Guid.NewGuid()
                    });
                    await _repositoryWrapper.Tag.SaveAsync();
                    tagLookup = await _repositoryWrapper.Tag.GetByConditionAsync(t => t.Name == tagTopicDto.Tag.Name);
                    tag = tagLookup.FirstOrDefault();
                }

                // add the tag guid to the output so that we can associate the course with these tags
                if (!tagGuids.Contains(tag.TagGuid.Value))
                    tagGuids.Add(tag.TagGuid.Value);

                // lookup topic (and create if necessary)
                var topicLookup = await _repositoryWrapper.Topic.GetByConditionAsync(t => t.Name == tagTopicDto.Topic.Name);
                var topic = topicLookup.FirstOrDefault();
                if (topic == null)
                {
                    await _repositoryWrapper.Topic.Create(new Topic()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        Description = tagTopicDto.Topic.Description,
                        IsDeleted = 0,
                        Name = tagTopicDto.Topic.Name,
                        Slug = string.Empty,
                        TopicGuid = Guid.NewGuid()
                    });
                    await _repositoryWrapper.Topic.SaveAsync();
                    topicLookup = await _repositoryWrapper.Topic.GetByConditionAsync(t => t.Name == tagTopicDto.Topic.Name);
                    topic = topicLookup.FirstOrDefault();
                }

                // create tag topic
                var tagTopicLookup = await _repositoryWrapper.TagTopic.GetByConditionAsync(tt => tt.TagId == tag.TagId && tt.TopicId == topic.TopicId);
                var tagTopic = tagTopicLookup.FirstOrDefault();
                if (tagTopic == null)
                {
                    await _repositoryWrapper.TagTopic.Create(new TagTopic()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        TagId = tag.TagId,
                        TopicId = topic.TopicId,
                        TagTopicGuid = Guid.NewGuid()
                    });
                    await _repositoryWrapper.TagTopic.SaveAsync();
                }
            }

            return tagGuids;
        }



        private async Task<Guid> SaveCourseReferral(CourseReferralDto courseReferralDto)
        {
            Guid courseReferralGuid = Guid.Empty;
      
            //get JobPostingId from JobPositngGuid
            var course = await _repositoryWrapper.Course.GetByGuid(courseReferralDto.CourseGuid);
           
            if (course == null)
                throw new NotFoundException("Course not found");
           
            //get ReferrerId from ReferrerGuid
            var referrer = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(courseReferralDto.ReferrerGuid);
           
           
            if (referrer == null)
                throw new NotFoundException("Referrer not found");
           
            //get ReferrerId from ReferrerGuid
            var referee = await _repositoryWrapper.SubscriberRepository.GetSubscriberByEmailAsync(courseReferralDto.ReferralEmail);
           
            //create JobReferral
            CourseReferral courseReferral = new CourseReferral()
            {
                CourseReferralGuid = Guid.NewGuid(),
                CourseId = course.CourseId,
                ReferrerId = referrer.SubscriberId,
                RefereeId = referee?.SubscriberId,
                RefereeEmail = courseReferralDto.ReferralEmail,
                IsCourseViewed = false
            };
           
            //set defaults
            BaseModelFactory.SetDefaultsForAddNew(courseReferral);
                  
            courseReferralGuid = await _repositoryWrapper.CourseReferralRepository.AddCourseReferralAsync(courseReferral);
        
            return courseReferralGuid;
        }

        private void SendReferralEmail(CourseReferralDto courseReferralDto, Guid courseReferralGuid)
        {

            var referralUrl = $"{_config["Environment:BaseUrl"].TrimEnd('/')}/course/{courseReferralDto.CourseGuid}";
            _hangfireService.Enqueue(() => _sysEmail.SendTemplatedEmailAsync(
                courseReferralDto.ReferralName,
                _config["SysEmail:Transactional:TemplateIds:CourseReferral-ReferAFriend"],
                new
                {
                    firstName = courseReferralDto.ReferralName,
                    description = courseReferralDto.ReferralDescription,
                    courseUrl = referralUrl
                },
               Constants.SendGridAccount.Transactional,
               null,
               null,
               null,
               null
                ));
             
        }


        #endregion

        #region Refer A Friend 



        public async Task<Guid> ReferCourseToFriend(Guid subscriberGuid, CourseReferralDto courseReferralDto)
        {
            courseReferralDto.ReferrerGuid = subscriberGuid;

            var courseReferralGuid = await SaveCourseReferral(courseReferralDto);
            SendReferralEmail(courseReferralDto, courseReferralGuid);

            return courseReferralGuid;
        }


        #endregion
    }
}
