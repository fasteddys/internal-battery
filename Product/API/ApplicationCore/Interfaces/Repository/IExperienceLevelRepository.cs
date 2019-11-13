using UpDiddyApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IExperienceLevelRepository : IUpDiddyRepositoryBase<ExperienceLevel>
    {
        Task<List<ExperienceLevel>> GetAllExperienceLevels();
    }
}
