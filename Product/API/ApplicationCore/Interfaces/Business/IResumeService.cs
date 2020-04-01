using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IResumeService
    {
        Task<bool> HasSubscriberUploadedResume(Guid subscriberGuid);
        Task<Guid> UploadResume(Guid subscriberGuid, FileDto fileDto);
        Task<FileDto> DownloadResume(Guid subscriberGuid);
        Task<Guid> GetResumeParse(Guid subscriberGuid);
        Task<UpDiddyLib.Dto.ResumeParseQuestionnaireDto> GetResumeQuestions(Guid subscriberGuid, Guid resumeParseGuid);
        Task ResolveProfileMerge(List<string> mergeInfo, Guid subscriberGuid, Guid resumeParseGuid);
        Task DeleteResume(Guid subscriberGuid);
    }
}
