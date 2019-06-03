using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
  
    public class ResumeParseRepository : UpDiddyRepositoryBase<ResumeParse>, IResumeParseRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public ResumeParseRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<ResumeParse> GetResumeParseByGuid(Guid resumeParseGuid)
        {
            var queryable = await GetAllAsync();
            var result  = queryable
                                .Where(jp => jp.IsDeleted == 0 && jp.ResumeParseGuid == resumeParseGuid)
                                .ToList();

            return result.Count == 0 ? null : result[0];
        }
    }
}
