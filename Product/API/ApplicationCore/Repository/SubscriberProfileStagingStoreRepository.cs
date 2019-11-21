using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class SubscriberProfileStagingStoreRepository : UpDiddyRepositoryBase<SubscriberProfileStagingStore>, ISubscriberProfileStagingStoreRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberProfileStagingStoreRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

    }
}
