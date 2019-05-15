using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto.Reporting;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IReportingService
    {
        Task<List<JobApplicationCountDto>> GetApplicationCountPerCompanyByDates(Guid? companyGuid, DateTime? startDate, DateTime? endDate);
    }
}
