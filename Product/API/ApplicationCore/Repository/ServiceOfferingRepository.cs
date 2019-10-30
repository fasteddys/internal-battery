using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
 


namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ServiceOfferingRepository : UpDiddyRepositoryBase<Models.ServiceOffering>, IServiceOfferingRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public ServiceOfferingRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceOffering> GetByNameAsync(string name)
        {
            return await (from a in _dbContext.ServiceOffering
                          where a.Name == name && a.IsDeleted == 0
                          select a).FirstOrDefaultAsync();
        }

        public async Task<ServiceOffering> GetByGuidAsync(Guid guid)
        {
            return await (from a in _dbContext.ServiceOffering
                          where a.ServiceOfferingGuid == guid && a.IsDeleted == 0
                          select a).FirstOrDefaultAsync();
        }


        public  ServiceOffering GetByGuid(Guid guid)
        {
            return  (from a in _dbContext.ServiceOffering
                          where a.ServiceOfferingGuid == guid && a.IsDeleted == 0
                          select a).FirstOrDefault();
        }


        public async Task<IList<ServiceOffering>> GetAllServiceOfferings()
        {  
     
                var rVal = await _dbContext.ServiceOffering
                    .Where(s => s.IsDeleted == 0)
                    .Include(s => s.ServiceOfferingItems)
                    .ToListAsync();

                // order service offering items by their sort order 
                foreach ( ServiceOffering so in rVal)
                {
                    so.ServiceOfferingItems = so.ServiceOfferingItems.OrderBy(o => o.SortOrder).ToList();

                }
                return rVal;            
        }

    }
}
