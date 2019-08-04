using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SalesForceWaitListRepository :UpDiddyRepositoryBase<SalesForceWaitList>, ISalesForceWaitListRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SalesForceWaitListRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IQueryable<SalesForceWaitList>> GetAllSalesForceWaitListAsync()
        {
           return GetAllAsync();
        }
    }
}
