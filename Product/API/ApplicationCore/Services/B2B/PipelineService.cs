using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Services.B2B
{
        public class PipelineService : IPipelineService
        {
            private readonly IRepositoryWrapper _repositoryWrapper;
            private readonly IMapper _mapper;

            public PipelineService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
            {
                _repositoryWrapper = repositoryWrapper;
                _mapper = mapper;
            }

            public async Task<PipelineDto> GetPipelineForHiringManager(Guid pipelineGuid, Guid subscriberGuid)
            {
                if (pipelineGuid == null || pipelineGuid == Guid.Empty)
                    throw new FailedValidationException("pipelineGuid cannot be null or empty");

                PipelineDto pipelineDto;
                var pipeline = await _repositoryWrapper.PipelineRepository.GetPipelineForHiringManager(pipelineGuid, subscriberGuid);
                if (pipeline == null)
                    throw new NotFoundException("pipeline not found");

                return _mapper.Map<PipelineDto>(pipeline);
            }

            public async Task<PipelineProfileListDto> GetPipelineProfilesForHiringManager(Guid pipelineGuid, Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
            {
                if (pipelineGuid == null || pipelineGuid == Guid.Empty)
                    throw new FailedValidationException("pipelineGuid cannot be null or empty");

                var profilePipelines = await _repositoryWrapper.PipelineRepository.GetPipelineProfilesForHiringManager(pipelineGuid, subscriberGuid, limit, offset, sort, order);

                if (profilePipelines == null)
                    throw new NotFoundException("profile pipelines not found");
                return _mapper.Map<PipelineProfileListDto>(profilePipelines);
            }

            public async Task<Guid> CreatePipelineForHiringManager(Guid subscriberGuid, PipelineDto pipelineDto)
            {
                if (pipelineDto == null)
                    throw new FailedValidationException("pipelineDto cannot be null");

                return await _repositoryWrapper.PipelineRepository.CreatePipelineForHiringManager(subscriberGuid, pipelineDto);
            }

            public async Task UpdatePipelineForHiringManager(Guid subscriberGuid, PipelineDto pipelineDto)
            {
                if (pipelineDto == null)
                    throw new FailedValidationException("pipelineDto cannot be null");

                await _repositoryWrapper.PipelineRepository.UpdatePipelineForHiringManager(subscriberGuid, pipelineDto);
            }

            public async Task DeletePipelineForHiringManager(Guid subscriberGuid, Guid pipelineGuid)
            {
                if (pipelineGuid == null || pipelineGuid == Guid.Empty)
                    throw new FailedValidationException("pipelineGuid cannot be null or empty");

                await _repositoryWrapper.PipelineRepository.DeletePipelineForHiringManager(subscriberGuid, pipelineGuid);
            }

            public async Task<PipelineListDto> GetPipelinesForHiringManager(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
            {
                var pipelines = await _repositoryWrapper.PipelineRepository.GetPipelinesForHiringManager(subscriberGuid, limit, offset, sort, order);
                if (pipelines == null)
                    throw new NotFoundException("pipelines not found");
                return _mapper.Map<PipelineListDto>(pipelines);
            }

            public async Task<List<Guid>> AddPipelineProfilesForHiringManager(Guid subscriberGuid, Guid pipelineGuid, List<Guid> profileGuids)
            {
                if (pipelineGuid == null || pipelineGuid == Guid.Empty)
                    throw new FailedValidationException("pipelineGuid cannot be null or empty");

                if (profileGuids == null || profileGuids.Count() == 0)
                    throw new FailedValidationException("profileGuids cannot be null or empty");

                return await _repositoryWrapper.PipelineRepository.AddPipelineProfilesForHiringManager(subscriberGuid, pipelineGuid, profileGuids);
            }

            public async Task DeletePipelineProfilesForHiringManager(Guid subscriberGuid, List<Guid> profilePipelineGuids)
            {
                if (profilePipelineGuids == null || profilePipelineGuids.Count() == 0)
                    throw new FailedValidationException("profilePipelineGuids cannot be null or empty");

                await _repositoryWrapper.PipelineRepository.DeletePipelineProfilesForHiringManager(subscriberGuid, profilePipelineGuids);
            }
        }
}
