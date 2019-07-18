using System.Collections.Generic;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobPostingService
    {
        Task<List<KeyValuePair<int, int>>> GetJobCountPerProvinceAsync();
    }
}
