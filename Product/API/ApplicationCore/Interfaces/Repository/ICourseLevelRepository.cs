using UpDiddyApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ICourseLevelRepository : IUpDiddyRepositoryBase<CourseLevel>
    {
        Task<List<CourseLevel>> GetAllCourseLevels();
    }
}
