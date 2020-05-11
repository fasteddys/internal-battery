using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models.B2B;
using UpDiddyLib.Domain.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
  public interface IPipelineRepository : IUpDiddyRepositoryBase<Pipeline>
    {
        Task<Pipeline> GetPipelineForHiringManager(Guid PipelineGuid, Guid subscriberGuid);
        Task<List<PipelineProfileDto>> GetPipelineProfilesForHiringManager(Guid PipelineGuid, Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<Guid> CreatePipelineForHiringManager(Guid subscriberGuid, PipelineDto PipelineDto);
        Task UpdatePipelineForHiringManager(Guid subscriberGuid, PipelineDto PipelineDto);
        Task DeletePipelineForHiringManager(Guid subscriberGuid, Guid PipelineGuid);
        Task<List<PipelineDto>> GetPipelinesForHiringManager(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<Guid>> AddPipelineProfilesForHiringManager(Guid subscriberGuid, Guid PipelineGuid, List<Guid> profileGuids);
        Task DeletePipelineProfilesForHiringManager(Guid subscriberGuid, List<Guid> profilePipelineGuids);
    }
}