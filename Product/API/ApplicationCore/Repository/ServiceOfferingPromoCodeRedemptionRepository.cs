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
    public class ServiceOfferingPromoCodeRedemptionRepository : UpDiddyRepositoryBase<Models.ServiceOfferingPromoCodeRedemption>, IServiceOfferingPromoCodeRedemptionRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public ServiceOfferingPromoCodeRedemptionRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceOfferingPromoCodeRedemption> GetByGuidAsync(Guid guid)
        {
            /*
             * todo jab fix 
            return await (from a in _dbContext.ServiceOfferingPromoCodeRedemption
                          where a.ServiceOfferingPromoCodeRedemptionGuid == guid && a.IsDeleted == 0
                          select a).FirstOrDefaultAsync();
                        */
            return null;
        }


    }
}
