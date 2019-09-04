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
    public class ServiceOfferingItemRepository : UpDiddyRepositoryBase<Models.ServiceOfferingItem>, IServiceOfferingItemRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public ServiceOfferingItemRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceOfferingItem> GetByNameAsync(string name)
        {
            return await (from a in _dbContext.ServiceOfferingItem
                          where a.Name == name
                          select a).FirstOrDefaultAsync();
        }
    }
}
