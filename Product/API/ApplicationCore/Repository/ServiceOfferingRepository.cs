using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using System.Linq;
 


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
                          where a.Name == name
                          select a).FirstOrDefaultAsync();
        }

        public async Task<ServiceOffering> GetByGuidAsync(Guid guid)
        {
            return await (from a in _dbContext.ServiceOffering
                          where a.ServiceOfferingGuid == guid
                          select a).FirstOrDefaultAsync();
        }


        public  ServiceOffering GetByGuid(Guid guid)
        {
            return  (from a in _dbContext.ServiceOffering
                          where a.ServiceOfferingGuid == guid
                          select a).FirstOrDefault();
        }



        public IList<ServiceOffering> GetAllServiceOfferings()
        {  
            //todo jab remove try 
            try
            {
                var rVal = _dbContext.ServiceOffering
                    .Where(s => s.IsDeleted == 0)
                    .Include(s => s.ServiceOfferingItems)
                    .ToList();


                foreach ( ServiceOffering so in rVal)
                {
                    so.ServiceOfferingItems = so.ServiceOfferingItems.OrderBy(o => o.SortOrder).ToList();

                }


                return rVal;
            }
            catch ( Exception ex )
            {
                return null;
            }
            
        }



    }
}
