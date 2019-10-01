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
    public class ServiceOfferingOrderRepository : UpDiddyRepositoryBase<Models.ServiceOfferingOrder>, IServiceOfferingOrderRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public ServiceOfferingOrderRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceOfferingOrder> GetByGuidAsync(Guid guid)
        {
             
            return await (from a in _dbContext.ServiceOfferingOrder
                          where a.ServiceOfferingOrderGuid == guid && a.IsDeleted == 0 
                          select a).FirstOrDefaultAsync();
             
        }


    }
}
