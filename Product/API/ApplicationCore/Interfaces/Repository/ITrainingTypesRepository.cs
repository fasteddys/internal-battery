using UpDiddyApi.Models;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using System.Collections.Generic;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ITrainingTypesRepository : IUpDiddyRepositoryBase<TrainingType>
    {
        Task<List<TrainingType>> GetAllTrainingTypes(int limit, int offset, string sort, string order);
    }
}
