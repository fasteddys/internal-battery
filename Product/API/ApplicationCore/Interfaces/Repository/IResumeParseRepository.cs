using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IResumeParseRepository
    {
        Task<ResumeParse> GetResumeParseByGuid(Guid resumeParseGuid);
        Task<ResumeParse> CreateResumeParse(int subscriberId, int subscriberFileId);
        Task<bool> SaveResumeParse();
        Task<IList<ResumeParse>> GetResumeParseForSubscriber(int subscriberId);
        Task <ResumeParse> GetLatestResumeParseForSubscriber(int subscriberId);
        Task<bool> DeleteAllResumeParseForSubscriber(int subscriberId);
    }
}
