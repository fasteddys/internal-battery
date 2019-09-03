using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class PromoCodeRepository : UpDiddyRepositoryBase<Models.PromoCode>, IPromoCodeRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public PromoCodeRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public PromoCode GetByName(string name)
        {
            return  (from a in _dbContext.PromoCode
                         where a.PromoName == name
                         select a).FirstOrDefault();
        }

    }
}
