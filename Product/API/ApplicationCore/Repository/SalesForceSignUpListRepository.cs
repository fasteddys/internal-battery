using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SalesForceSignUpListRepository :UpDiddyRepositoryBase<SalesForceSignUpList>, ISalesForceSignUpListRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SalesForceSignUpListRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<SalesForceSignUpList> GetAllSalesForceSignUpListAsync()
        {
           return GetAll();
        }
    }
}
