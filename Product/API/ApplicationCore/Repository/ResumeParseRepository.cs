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

        UpDiddyDbContext _db = null;
        public ResumeParseRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _db = dbContext;
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


 

        public async Task <IList<ResumeParse>> GetResumeParseForSubscriber(int subscriberId)
        {
            var parses = _db.ResumeParse
                .Where(rp => rp.IsDeleted == 0 && rp.SubscriberId == subscriberId)
                .OrderByDescending(rp => rp.CreateDate)
                .ToList();

            return parses;
        }

        public async Task<ResumeParse> GetLatestResumeParseForSubscriber(int subscriberId)
        {
            return _db.ResumeParse
                .Where(rp => rp.IsDeleted == 0 && rp.SubscriberId == subscriberId)
                .OrderByDescending(rp => rp.CreateDate)
                .FirstOrDefault();
        }

        public async Task<ResumeParse> GetLatestResumeParseRequiringMergeForSubscriber(int subscriberId)
        {
            return _db.ResumeParse
                .Where(rp => rp.IsDeleted == 0 && rp.SubscriberId == subscriberId && rp.RequiresMerge == 1)
                .OrderByDescending(rp => rp.CreateDate)
                .FirstOrDefault();
        }



        public async Task<bool> DeleteAllResumeParseForSubscriber(int subscriberId)
        {
            var parses = await GetResumeParseForSubscriber(subscriberId);
            foreach (ResumeParse rp in parses)
                rp.IsDeleted = 1;

            await SaveAsync();

            return true;
        }


        public async Task<ResumeParse> GetResumeParseByGuid(Guid resumeParseGuid)
        {

            var result = _db.ResumeParse
                          .Where(jp => jp.IsDeleted == 0 && jp.ResumeParseGuid == resumeParseGuid)
                          .FirstOrDefault();

            return result;
        }
    }
}
