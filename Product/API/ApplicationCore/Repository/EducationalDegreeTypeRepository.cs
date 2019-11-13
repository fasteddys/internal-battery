using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EducationalDegreeTypeRepository : UpDiddyRepositoryBase<EducationalDegreeType>, IEducationalDegreeTypeRepository
    {
        public EducationalDegreeTypeRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
