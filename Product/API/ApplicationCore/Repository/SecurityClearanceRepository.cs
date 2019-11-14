using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SecurityClearanceRepository : UpDiddyRepositoryBase<SecurityClearance>, ISecurityClearanceRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SecurityClearanceRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SecurityClearance>> GetAllSecurityClearances()
        {
            return await (from e in _dbContext.SecurityClearance
                          where e.IsDeleted == 0
                          select e).ToListAsync();
        }
    }
}
