using UpDiddyApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IEmploymentTypeRepository : IUpDiddyRepositoryBase<EmploymentType>
    {
        Task<List<EmploymentType>> GetAllEmploymentTypes();
    }
}
