using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobSearchService
    {
        Task<int> GetActiveJobCount();
        Task<List<UpDiddyLib.Domain.Models.JobPostingDto>> GetSimilarJobs(Guid jobPostingGuid);
        Task<List<StateMapDto>> GetStateMapData();
    }
}
