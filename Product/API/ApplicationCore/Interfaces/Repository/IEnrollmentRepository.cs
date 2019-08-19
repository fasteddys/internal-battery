using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IEnrollmentRepository : IUpDiddyRepositoryBase<Enrollment>
    {
        Task<int> GetEnrollmentsCountByStartEndDates(DateTime? startDate=null, DateTime? endDate=null);
    }
}