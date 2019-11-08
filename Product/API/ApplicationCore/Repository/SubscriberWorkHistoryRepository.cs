using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberWorkHistoryRepository : UpDiddyRepositoryBase<SubscriberWorkHistory>, ISubscriberWorkHistoryRepository
    {
        public SubscriberWorkHistoryRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
