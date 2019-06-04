﻿using System;
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

        public ResumeParseResultRespository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }


        public async Task<ResumeParseResult> GetResumeParseResultByGuidAsync(Guid resumeParseResultGuid)
        {
            var queryable = await GetAllAsync();
            var result = queryable
                                .Where(jp => jp.IsDeleted == 0 && jp.ResumeParseResultGuid == resumeParseResultGuid)
                                .ToList();

            return result.Count == 0 ? null : result[0];

        }
        public async Task<ResumeParseResult> CreateResumeParseResultAsync(int resumeParseId, string prompt, string targetTypeName, string targetProperty, string existingValue, string parsedValue, int status, Guid existingObjectGuid)
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
                Prompt = prompt
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
