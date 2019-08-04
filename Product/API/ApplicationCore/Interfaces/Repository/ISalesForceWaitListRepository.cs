using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISalesForceWaitListRepository : IUpDiddyRepositoryBase<SalesForceWaitList>
    {
        Task<IQueryable<SalesForceWaitList>> GetAllSalesForceWaitListAsync();
    }
}
