using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IResumeParseResultRepository
    {
        Task<ResumeParseResult> GetResumeParseResultByGuidAsync(Guid resumeParseGuid);
        Task<ResumeParseResult> CreateResumeParseResultAsync(int resumeParseId,  string prompt, string targetTypeName, string targetProperty, string existingValue, string parsedValue, int status, Guid objectGuid);
        Task<bool> SaveResumeParseResultAsync();
    }
}
