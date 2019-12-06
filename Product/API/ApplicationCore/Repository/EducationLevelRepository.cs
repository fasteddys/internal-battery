using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EducationLevelRepository : UpDiddyRepositoryBase<EducationLevel>, IEducationLevelRepository
    {
        public EducationLevelRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
