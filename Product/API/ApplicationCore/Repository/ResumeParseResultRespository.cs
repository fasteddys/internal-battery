using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ResumeParseResultRespository : UpDiddyRepositoryBase<ResumeParseResult>, IResumeParseResultRepository    
    {

        private readonly UpDiddyDbContext _db;

        public ResumeParseResultRespository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _db = dbContext;
        }


        public async Task<IList<ResumeParseResult>> GetResumeParseResultsForResumeParseById(int resumeParseId)
        {
            return _db.ResumeParseResult                 
                    .Where(rp => rp.IsDeleted == 0 && rp.ResumeParseId == resumeParseId && rp.ParseStatus == (int) ResumeParseStatus.MergeNeeded)
                    .ToList();
        }

        public async Task<ResumeParseResult> GetResumeParseResultByGuidAsync(Guid resumeParseResultGuid)
        {

            var result = _db.ResumeParseResult
                                .Where(jp => jp.IsDeleted == 0 && jp.ResumeParseResultGuid == resumeParseResultGuid)
                                .FirstOrDefault();

            return result;
        }
        public async Task<ResumeParseResult> CreateResumeParseResultAsync(int resumeParseId, int profileSectionId, string prompt, string targetTypeName, string targetProperty, string existingValue, string parsedValue, int status, Guid existingObjectGuid)
        {
            ResumeParseResult resumeParseResult = new ResumeParseResult()
            {
                IsDeleted = 0,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                CreateGuid = Guid.NewGuid(),
                ModifyGuid = Guid.NewGuid(),
                ResumeParseResultGuid = Guid.NewGuid(),
                ResumeParseId = resumeParseId,
                ParseStatus = status,
                TargetTypeName = targetTypeName,
                TargetProperty = targetProperty,
                ExistingValue = existingValue,
                ParsedValue = parsedValue,
                ExistingObjectGuid = existingObjectGuid,
                Prompt = prompt,
                ProfileSectionId = profileSectionId
            };

            await Create(resumeParseResult); 

            return resumeParseResult;

        }

        public async Task<bool> SaveResumeParseResultAsync()
        {
            await SaveAsync();
            return true;
        }




    }
}
