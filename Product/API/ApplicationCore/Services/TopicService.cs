using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Exceptions;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Helpers;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class TopicService : ITopicService
    {
        private IRepositoryWrapper _repositoryWrapper { get; set; }
        private IConfiguration _configuration { get; set; }
        private readonly IMapper _mapper;

        public TopicService(
            IRepositoryWrapper repository,
            IMapper mapper,
            IConfiguration configuration)
        {
            _repositoryWrapper = repository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<List<UpDiddyLib.Domain.Models.TopicDto>> GetTopics(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var careerPaths = await _repositoryWrapper.Topic.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            return _mapper.Map<List<UpDiddyLib.Domain.Models.TopicDto>>(careerPaths);
        }

        public async Task<List<TopicCourseDto>> GetTopicCourses(Guid topicGuid)
        {
            if (topicGuid == null || topicGuid == Guid.Empty)
                throw new NullReferenceException("TopicGuid cannot be null");
            var courses = await _repositoryWrapper.Course.GetCoursesByTopicGuid(topicGuid);
            if (courses == null)
                throw new NotFoundException("TopicGuid not found");
            var coursesDto = _mapper.Map<List<TopicCourseDto>>(courses);
            CourseUrlHelper.AssignVendorLogoUrlToCourse(coursesDto,_configuration);
            return coursesDto;
        }

        public async Task<List<UpDiddyLib.Domain.Models.SkillDto>> GetTopicSkills(Guid topicGuid)
        {
            if (topicGuid == null || topicGuid == Guid.Empty)
                throw new NullReferenceException("topicGuid cannot be null");
            var skills = await _repositoryWrapper.SkillRepository.GetByTopicGuid(topicGuid);
            if (skills == null)
                throw new NotFoundException("Skills not found");
            return _mapper.Map<List<UpDiddyLib.Domain.Models.SkillDto>>(skills);
        }

    }
}
