using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EmploymentTypeRepository : UpDiddyRepositoryBase<EmploymentType>, IEmploymentTypeRepository
    {
        public EmploymentTypeRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
