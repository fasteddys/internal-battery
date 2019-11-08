using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EducationalInstitutionRepository : UpDiddyRepositoryBase<EducationalInstitution>, IEducationalInstitutionRepository
    {
        public EducationalInstitutionRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
