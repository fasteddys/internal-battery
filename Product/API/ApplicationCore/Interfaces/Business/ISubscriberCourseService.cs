using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISubscriberCourseService
    {
        Task<List<SubscriberCourseDto>> GetSubscriberCourses(Guid subscriberGuid, Guid talentGuid, int ExcludeActive, int ExcludeCompleted, bool isRecruiter);

    }
}
