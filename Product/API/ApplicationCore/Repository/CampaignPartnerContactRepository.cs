using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CampaignPartnerContactRepository : UpDiddyRepositoryBase<CampaignPartnerContact>, ICampaignPartnerContactRepository
    {
        public CampaignPartnerContactRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
