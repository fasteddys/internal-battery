using AutoMapper;
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

        public async Task AddCourseAsync(CourseDto courseDto)
        {
            // process tag topics - note that we do not await here; it is acceptable to continue processing course while tag topics are being processed
            var tagGuids = ProcessTagTopicsAsync(courseDto.TagTopics);

            // lookup skills and create those that do not exist
            var skillGuids = GetSkillGuidsAsync(courseDto.Skills);



            courseDto.



            // create course skill

            // create course tag

            // create course topic

            // lookup vendor

            // create course
            _repositoryWrapper.Course.Create(new Course()
            {

            });

            // lookup course variant type
            var courseVariantTypeLookup = await _repositoryWrapper.CourseVariantType.GetByConditionAsync(cvt => cvt.Name == "Self-Paced");
            var selfPacedCourseVariant = courseVariantTypeLookup.FirstOrDefault();

            // create course variant
            _repositoryWrapper.CourseVariant.Create(
                new CourseVariant()
                {
                    CourseVariantGuid = Guid.NewGuid(),
                    CourseVariantTypeId = selfPacedCourseVariant.CourseVariantTypeId,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    CourseId = 1, // todo: replace with actual value from previous operation
                    IsDeleted = 0,
                    Price = 0.00M
                });

            throw new NotImplementedException();
        }

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

        public async Task EditCourseAsync(CourseDto courseDto)
        {
            // process tag topics - note that we do not await here; it is acceptable to continue processing course while tag topics are being processed
            ProcessTagTopicsAsync(courseDto.TagTopics);


            throw new NotImplementedException();
        }

        public async Task<List<CourseDto>> GetCoursesAsync()
        {
            // no use for this right now
            throw new NotImplementedException();
        }

        #region Private Members

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
                if (skill.SkillGuid.HasValue)
                    skillGuids.Add(skill.SkillGuid.Value);
            }
            return skillGuids;
        }

        /// <summary>
        /// Handles the creation of tags, topics, and tag topics. 
        /// This process does belong here as opposed to some other service because the implementation of tags and topics is specific to courses.
        /// </summary>
        /// <param name="tagTopics"></param>
        /// <returns></returns>
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
