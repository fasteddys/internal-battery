using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class RecruiterCompanyRepository : UpDiddyRepositoryBase<RecruiterCompany>, IRecruiterCompanyRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public RecruiterCompanyRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
