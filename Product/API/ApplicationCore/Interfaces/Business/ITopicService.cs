using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITopicService
    {
        Task<List<UpDiddyLib.Domain.Models.TopicDto>> GetTopics(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<List<TopicCourseDto>> GetTopicCourses(Guid topicGuid);
        Task<List<UpDiddyLib.Domain.Models.SkillDto>> GetTopicSkills(Guid topicGuid);
    }
}
