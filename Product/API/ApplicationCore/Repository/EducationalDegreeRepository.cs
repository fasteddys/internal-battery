using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EducationalDegreeRepository : UpDiddyRepositoryBase<EducationalDegree>, IEducationalDegreeRepository
    {
        public EducationalDegreeRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
