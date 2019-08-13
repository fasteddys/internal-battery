using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberFileRepository : UpDiddyRepositoryBase<SubscriberFile>, ISubscriberFileRepository
    {
        public SubscriberFileRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }
        public IQueryable<SubscriberFile> GetAllSubscriberFileQueryableAsync()
        {
           return GetAll();
        }

        public async Task UpdateSubscriberFileAsync(SubscriberFile subscriberFile)
        {
            Update(subscriberFile);
            await SaveAsync();
        }
    }
}
