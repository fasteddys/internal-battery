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
    public class EducationalDegreeTypeService : IEducationalDegreeTypeService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IMapper _mapper;
        public EducationalDegreeTypeService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IMemoryCacheService memoryCacheService)
        {
            _repositoryWrapper = repositoryWrapper;
            _memoryCacheService = memoryCacheService;
            _mapper = mapper;
        }

        public async Task<EducationalDegreeTypeDto> GetEducationalDegreeType(Guid educationalDegreeTypeGuid)
        {
            if (educationalDegreeTypeGuid == null || educationalDegreeTypeGuid == Guid.Empty)
                throw new NullReferenceException("EducationalDegreeTypeGuid cannot be null");
            string cacheKey = $"GetEducationalDegreeType";
            IList<EducationalDegreeTypeDto> rval = (IList<EducationalDegreeTypeDto>)_memoryCacheService.GetCacheValue(cacheKey);
            if (rval == null)
            {
                var educationalDegreeTypes = await _repositoryWrapper.EducationalDegreeTypeRepository.GetAllEducationDegreeTypes();
                if (educationalDegreeTypes == null)
                    throw new NotFoundException("EducationalDegreeTypes not found");
                rval = _mapper.Map<List<EducationalDegreeTypeDto>>(educationalDegreeTypes);
                _memoryCacheService.SetCacheValue(cacheKey, rval);
            }
            return rval?.Where(x => x.EducationalDegreeTypeGuid == educationalDegreeTypeGuid).FirstOrDefault();
        }

        public async Task<List<EducationalDegreeTypeDto>> GetAllEducationDegreeTypes()
        {
            string cacheKey = $"GetEducationalDegreeType";
            IList<EducationalDegreeTypeDto> rval = (IList<EducationalDegreeTypeDto>)_memoryCacheService.GetCacheValue(cacheKey);
            if (rval == null)
            {
                var educationalDegreeTypes = await _repositoryWrapper.EducationalDegreeTypeRepository.GetAllEducationDegreeTypes();
                if (educationalDegreeTypes == null)
                    throw new NotFoundException("EducationalDegreeTypes not found");
                rval = _mapper.Map<List<EducationalDegreeTypeDto>>(educationalDegreeTypes);
                _memoryCacheService.SetCacheValue(cacheKey, rval);
            }
            return rval.ToList();
        }

        public async Task<List<EducationalDegreeTypeDto>> GetEducationalDegreeTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var educationalDegreeTypes = await _repositoryWrapper.EducationalDegreeTypeRepository.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            if (educationalDegreeTypes == null)
                throw new NotFoundException("EducationalDegreeTypes not found");
            return _mapper.Map<List<EducationalDegreeTypeDto>>(educationalDegreeTypes);
        }

        public async Task CreateEducationalDegreeType(EducationalDegreeTypeDto educationalDegreeTypeDto)
        {
            if (educationalDegreeTypeDto == null)
                throw new NullReferenceException("EducationalDegreeTypeDto cannot be null");
            var educationalDegreeType = _mapper.Map<EducationalDegreeType>(educationalDegreeTypeDto);
            educationalDegreeType.CreateDate = DateTime.UtcNow;
            educationalDegreeType.EducationalDegreeTypeGuid = Guid.NewGuid();
            await _repositoryWrapper.EducationalDegreeTypeRepository.Create(educationalDegreeType);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdateEducationalDegreeType(Guid educationalDegreeTypeGuid, EducationalDegreeTypeDto educationalDegreeTypeDto)
        {
            if (educationalDegreeTypeDto == null || educationalDegreeTypeGuid == null || educationalDegreeTypeGuid == Guid.Empty)
                throw new NullReferenceException("EducationalDegreeTypeDto and EducationalDegreeTypeGuid cannot be null");
            var educationalDegreeType = await _repositoryWrapper.EducationalDegreeTypeRepository.GetByGuid(educationalDegreeTypeGuid);
            if (educationalDegreeType == null)
                throw new NotFoundException("EducationalDegreeType not found");
            educationalDegreeType.DegreeType = educationalDegreeTypeDto.DegreeType;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteEducationalDegreeType(Guid educationalDegreeTypeGuid)
        {
            if (educationalDegreeTypeGuid == null || educationalDegreeTypeGuid == Guid.Empty)
                throw new NullReferenceException("CountryGuid cannot be null");
            var educationalDegreeType = await _repositoryWrapper.EducationalDegreeTypeRepository.GetByGuid(educationalDegreeTypeGuid);
            if (educationalDegreeType == null)
                throw new NotFoundException("EducationalDegreeType not found");
            educationalDegreeType.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
