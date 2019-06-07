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
  
    public class ResumeParseRepository : UpDiddyRepositoryBase<ResumeParse>, IResumeParseRepository
    {
      
        public ResumeParseRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
 
        }

        public async Task<ResumeParse> CreateResumeParse(int subscriberId, int subscriberFileId)
        {            
            ResumeParse resumeParse =  new ResumeParse()
            {
                IsDeleted = 0,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                CreateGuid = Guid.NewGuid(),
                ModifyGuid = Guid.NewGuid(),
                ResumeParseGuid = Guid.NewGuid(),
                SubscriberId = subscriberId,
                SubscriberFileId = subscriberFileId,
                ParseStatus = (int) ResumeParseStatus.MergeNeeded,
                RequiresMerge = 0

            };
            await Create(resumeParse);
            return resumeParse;
        }


        public async Task<bool> SaveResumeParse()
        {
            await SaveAsync();
            return true;
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
