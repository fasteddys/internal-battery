using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IResumeParseResultRepository
    {
        Task<ResumeParseResult> GetResumeParseResultByGuidAsync(Guid resumeParseResultGuid);
        Task<ResumeParseResult> CreateResumeParseResultAsync(int resumeParseId, int profileSectionId, string prompt, string targetTypeName, string targetProperty, string existingValue, string parsedValue, int status, Guid objectGuid);
        Task<bool> SaveResumeParseResultAsync();

        Task<IList<ResumeParseResult>> GetResultsRequiringMergeById(int ResumeParseId);



    }
}
