using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.B2B
{
    public interface IPipelineService
    {
        Task<PipelineDto> GetPipelineForHiringManager(Guid PipelineGuid, Guid subscriberGuid);
        Task<PipelineProfileListDto> GetPipelineProfilesForHiringManager(Guid PipelineGuid, Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<Guid> CreatePipelineForHiringManager(Guid subscriberGuid, PipelineDto PipelineDto);
        Task UpdatePipelineForHiringManager(Guid subscriberGuid, PipelineDto PipelineDto);
        Task DeletePipelineForHiringManager(Guid subscriberGuid, Guid PipelineGuid);
        Task<PipelineListDto> GetPipelinesForHiringManager(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task DeletePipelineProfilesForHiringManager(Guid subscriberGuid, List<Guid> profilePipelineGuids);
        Task<List<Guid>> AddPipelineProfilesForHiringManager(Guid subscriberGuid, Guid PipelineGuid, List<Guid> profileGuids);
    }
}