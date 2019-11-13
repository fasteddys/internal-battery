using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CompensationTypeRepository : UpDiddyRepositoryBase<CompensationType>, ICompensationTypeRepository
    {
        public CompensationTypeRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
