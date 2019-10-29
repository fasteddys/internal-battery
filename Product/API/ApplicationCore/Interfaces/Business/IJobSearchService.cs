using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobSearchService
    {
        Task<int> GetActiveJobCount();
    }
}
