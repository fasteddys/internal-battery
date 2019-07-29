using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobPostingService
    {
        Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync();
    }
}
