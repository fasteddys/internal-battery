using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobPostingService
    {
        Task<List<KeyValuePair<int, int>>> GetJobCountPerProvinceAsync1();
        Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync();
    }
}
