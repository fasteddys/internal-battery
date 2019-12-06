using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITopicService
    {
        Task<List<UpDiddyLib.Domain.Models.TopicDto>> GetTopics();
        Task<List<TopicCourseDto>> GetTopicCourses(Guid topicGuid);
        Task<List<UpDiddyLib.Domain.Models.SkillDto>> GetTopicSkills(Guid topicGuid);
    }
}
