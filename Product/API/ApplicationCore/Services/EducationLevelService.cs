using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;
using AutoMapper;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Exceptions;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class EducationLevelService : IEducationLevelService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public EducationLevelService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<EducationLevelListDto> GetEducationLevels(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var educationLevels = await _repositoryWrapper.StoredProcedureRepository.GetEducationLevels(limit, offset, sort, order);
            if (educationLevels == null)
                throw new NotFoundException("educationLevels not found");
            return _mapper.Map<EducationLevelListDto>(educationLevels);
        }

        public async Task<EducationLevelDto> GetEducationLevel(Guid educationLevel)
        {
            if (educationLevel == null || educationLevel == Guid.Empty)
                throw new NullReferenceException("educationLevel cannot be null");
            EducationLevelDto rval;
            var existingEducationLevel = await _repositoryWrapper.EducationLevelRepository.GetByGuid(educationLevel);
            if (existingEducationLevel == null)
                throw new NotFoundException("educationLevel not found");
            return _mapper.Map<EducationLevelDto>(existingEducationLevel);
        }

        public async Task<Guid> CreateEducationLevel(EducationLevelDto educationLevelDto)
        {
            if (educationLevelDto == null)
                throw new NullReferenceException("educationLevelDto cannot be null");
            var educationLevel = _mapper.Map<EducationLevel>(educationLevelDto);
            educationLevel.CreateDate = DateTime.UtcNow;
            educationLevel.EducationLevelGuid = Guid.NewGuid();
            await _repositoryWrapper.EducationLevelRepository.Create(educationLevel);
            await _repositoryWrapper.SaveAsync();
            return educationLevel.EducationLevelGuid;
        }

        public async Task UpdateEducationLevel(Guid educationLevel, EducationLevelDto educationLevelDto)
        {
            if (educationLevelDto == null || educationLevel == null || educationLevel == Guid.Empty)
                throw new NullReferenceException("educationLevelDto and educationLevelGuid cannot be null");
            var existingEducationLevel = await _repositoryWrapper.EducationLevelRepository.GetByGuid(educationLevel);
            if (existingEducationLevel == null)
                throw new NotFoundException("educationLevel not found");
            existingEducationLevel.Level = educationLevelDto.Level;
            existingEducationLevel.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteEducationLevel(Guid educationLevel)
        {
            if (educationLevel == null || educationLevel == Guid.Empty)
                throw new NullReferenceException("educationLevel cannot be null");
            var existingEducationLevel = await _repositoryWrapper.EducationLevelRepository.GetByGuid(educationLevel);
            if (existingEducationLevel == null)
                throw new NotFoundException("educationLevel not found");
            existingEducationLevel.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
