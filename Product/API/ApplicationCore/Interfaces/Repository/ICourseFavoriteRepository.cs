using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICourseFavoriteRepository : IUpDiddyRepositoryBase<CourseFavorite>
    {
        Task<CourseFavorite> GetBySubscriberGuidAndCourseGuid(Guid subscriberGuid, Guid courseGuid);
    }
}
