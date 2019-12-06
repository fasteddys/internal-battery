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
    public class ExperienceLevelService : IExperienceLevelService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IMapper _mapper;
        public ExperienceLevelService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IMemoryCacheService memoryCacheService)
        {
            _repositoryWrapper = repositoryWrapper;
            _memoryCacheService = memoryCacheService;
            _mapper = mapper;
        }

        public async Task<ExperienceLevelDto> GetExperienceLevel(Guid experienceLevelGuid)
        {
            if (experienceLevelGuid == null || experienceLevelGuid == Guid.Empty)
                throw new NullReferenceException("ExperienceLevelGuid cannot be null");
            string cacheKey = $"GetExperienceLevel";
            IList<ExperienceLevelDto> rval = (IList<ExperienceLevelDto>)_memoryCacheService.GetCacheValue(cacheKey);
            if (rval == null)
            {
                var experienceLevels = await _repositoryWrapper.ExperienceLevelRepository.GetAllExperienceLevels();
                if (experienceLevels == null)
                    throw new NotFoundException("ExperienceLevelGuid not found");
                rval = _mapper.Map<List<ExperienceLevelDto>>(experienceLevels);
                _memoryCacheService.SetCacheValue(cacheKey, rval);
            }
            return rval?.Where(x => x.ExperienceLevelGuid == experienceLevelGuid).FirstOrDefault();
        }

        public async Task<List<ExperienceLevelDto>> GetExperienceLevels(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var experienceLevels = await _repositoryWrapper.ExperienceLevelRepository.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            if (experienceLevels == null)
                throw new NotFoundException("ExperienceLevels not found");
            return _mapper.Map<List<ExperienceLevelDto>>(experienceLevels);
        }

        public async Task CreateExperienceLevel(ExperienceLevelDto experienceLevelDto)
        {
            if (experienceLevelDto == null)
                throw new NullReferenceException("ExperienceLevelDto cannot be null");
            var experienceLevel = _mapper.Map<ExperienceLevel>(experienceLevelDto);
            experienceLevel.CreateDate = DateTime.UtcNow;
            experienceLevel.ExperienceLevelGuid = Guid.NewGuid();
            await _repositoryWrapper.ExperienceLevelRepository.Create(experienceLevel);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdateExperienceLevel(Guid experienceLevelGuid, ExperienceLevelDto experienceLevelDto)
        {
            if (experienceLevelDto == null || experienceLevelGuid == null || experienceLevelGuid == Guid.Empty)
                throw new NullReferenceException("ExperienceLevelDto and ExperienceLevelGuid cannot be null");
            var experienceLevel = await _repositoryWrapper.ExperienceLevelRepository.GetByGuid(experienceLevelGuid);
            if (experienceLevel == null)
                throw new NotFoundException("ExperienceLevel not found");
            experienceLevel.DisplayName = experienceLevelDto.DisplayName;
            experienceLevel.Code = experienceLevelDto.Code;
            experienceLevel.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteExperienceLevel(Guid experienceLevelGuid)
        {
            if (experienceLevelGuid == null || experienceLevelGuid == Guid.Empty)
                throw new NullReferenceException("experienceLevelGuid cannot be null");
            var experienceLevel = await _repositoryWrapper.ExperienceLevelRepository.GetByGuid(experienceLevelGuid);
            if (experienceLevel == null)
                throw new NotFoundException("ExperienceLevel not found");
            experienceLevel.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
