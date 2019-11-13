using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SecurityClearanceRepository : UpDiddyRepositoryBase<SecurityClearance>, ISecurityClearanceRepository
    {
        public SecurityClearanceRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
