using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class CourseService : ICourseService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public CourseService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
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

        public async Task<List<CourseDto>> GetCoursesAsync()
        {
            // we do not need to implement this right now
            throw new NotImplementedException();
        }

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
                _repositoryWrapper.CourseVariantType.Create(new CourseVariantType()
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
                _repositoryWrapper.Vendor.Create(new Vendor()
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
        private async Task<List<Guid>> GetSkillGuidsAsync(List<SkillDto> skills)
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
                    _repositoryWrapper.SkillRepository.Create(new Skill()
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
                    _repositoryWrapper.Tag.Create(new Tag()
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
                    _repositoryWrapper.Topic.Create(new Topic()
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
                    _repositoryWrapper.TagTopic.Create(new TagTopic()
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

        #endregion
    }
}
