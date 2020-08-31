using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ProhibitiedEmailDomainRepository : UpDiddyRepositoryBase<ProhibitiedEmailDomain>, IProhibitiedEmailDomainRepository
    {
        public ProhibitiedEmailDomainRepository(UpDiddyDbContext dbContext)
            : base(dbContext)
        { }
    }
}
