using UpDiddyApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICompensationTypeRepository : IUpDiddyRepositoryBase<CompensationType>
    {
        Task<List<CompensationType>> GetAllCompensationTypes();
    }
}
