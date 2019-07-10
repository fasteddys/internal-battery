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

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class EntityTypeRepository : UpDiddyRepositoryBase<EntityType>, IEntityTypeRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public EntityTypeRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<EntityType> GetByNameAsync(string entityTypeName)
        {
            return (from a in _dbContext.EntityType
                    where a.Name == entityTypeName
                    select a).FirstOrDefaultAsync();
        }
    }
}
