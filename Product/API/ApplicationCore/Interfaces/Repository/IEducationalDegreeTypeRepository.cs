using UpDiddyApi.Models;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using System.Collections.Generic;
namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IEducationalDegreeTypeRepository : IUpDiddyRepositoryBase<EducationalDegreeType>
    {
        Task<List<EducationalDegreeType>> GetAllEducationDegreeTypes();
    }
}
