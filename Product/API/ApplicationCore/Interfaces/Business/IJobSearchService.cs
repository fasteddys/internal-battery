using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobSearchService
    {
        Task<int> GetActiveJobCount();
        Task<List<JobPostingDto>> GetSimilarJobs(Guid jobPostingGuid, int limit, int offset, string sort, string order);
    }
}
