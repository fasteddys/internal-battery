using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITopicService
    {
        Task<List<TopicDto>> GetTopics();
        Task<List<CourseDetailDto>> GetTopicCourses(Guid topicGuid);
        Task<List<SkillDto>> GetTopicSkills(Guid topicGuid);
    }
}
