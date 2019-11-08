using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class IndustryRepository : UpDiddyRepositoryBase<Industry>, IIndustryRepository
    {
        public IndustryRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
