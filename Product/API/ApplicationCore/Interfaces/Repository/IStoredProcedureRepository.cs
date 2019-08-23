using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IStoredProcedureRepository
    {
        Task<List<JobAbandonmentStatistics>> GetJobAbandonmentStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<List<JobCountPerProvince>> GetJobCountPerProvince();

        Task<List<SubscriberSourceDto>> GetSubscriberSources(int SubscriberId);
    }
}