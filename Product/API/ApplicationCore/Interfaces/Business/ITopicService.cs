using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITopicService
    {
        Task<TopicListDto> GetTopics(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<TopicDto> GetTopic(Guid topicGuid);
        Task<List<CourseDetailDto>> GetTopicCourses(Guid topicGuid);
        Task<List<SkillDto>> GetTopicSkills(Guid topicGuid);
    }
}
