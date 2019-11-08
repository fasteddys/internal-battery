using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ExperienceLevelRepository : UpDiddyRepositoryBase<ExperienceLevel>, IExperienceLevelRepository
    {
        public ExperienceLevelRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
