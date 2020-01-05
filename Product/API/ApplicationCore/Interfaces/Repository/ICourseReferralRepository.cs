using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICourseReferralRepository
    {
        Task<Guid> AddCourseReferralAsync(CourseReferral courseReferral);

        Task<CourseReferral> GetJobReferralByGuid(Guid courseReferralGuid);

        Task UpdateCourseReferral(CourseReferral courseReferral);

    }
}
