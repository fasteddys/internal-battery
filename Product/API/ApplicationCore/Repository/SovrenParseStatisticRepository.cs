using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SovrenParseStatisticRepository : UpDiddyRepositoryBase<SovrenParseStatistic>, ISovrenParseStatisticRepository
    {
        public SovrenParseStatisticRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
        }
    }

}