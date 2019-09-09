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
    public class ServiceOfferingPromoCodeRepository : UpDiddyRepositoryBase<Models.ServiceOfferingPromoCode>, IServiceOfferingPromoCodeRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public ServiceOfferingPromoCodeRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public List<ServiceOfferingPromoCode> GetByPromoCodesId(int PromoCodeId)
        {
            return (from a in _dbContext.ServiceOfferingPromoCode
                         where a.PromoCodeId == PromoCodeId && a.IsDeleted == 0
                         select a).ToList();
        }
    }
}
