using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Dto;


namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CrosschqRepository : UpDiddyRepositoryBase<ReferenceCheck>, ICrosschqRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public CrosschqRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
