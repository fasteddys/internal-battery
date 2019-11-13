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
    public class CompensationTypeService : ICompensationTypeService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IMapper _mapper;
        public CompensationTypeService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IMemoryCacheService memoryCacheService)
        {
            _repositoryWrapper = repositoryWrapper;
            _memoryCacheService = memoryCacheService;
            _mapper = mapper;
        }

        public async Task<CompensationTypeDto> GetCompensationType(Guid compensationTypeGuid)
        {
            if (compensationTypeGuid == null || compensationTypeGuid == Guid.Empty)
                throw new NullReferenceException("CompensationTypeGuid cannot be null");
            string cacheKey = $"GetCompensationType";
            IList<CompensationTypeDto> rval = (IList<CompensationTypeDto>)_memoryCacheService.GetCacheValue(cacheKey);
            if (rval == null)
            {
                var compensationTypes = await _repositoryWrapper.CompensationTypeRepository.GetAllCompensationTypes();
                if (compensationTypes == null)
                    throw new NotFoundException("CompensationTypeGuid not found");
                rval = _mapper.Map<List<CompensationTypeDto>>(compensationTypes);
                _memoryCacheService.SetCacheValue(cacheKey, rval);
            }
            return rval?.Where(x => x.CompensationTypeGuid == compensationTypeGuid).FirstOrDefault();
        }

        public async Task<List<CompensationTypeDto>> GetCompensationTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var compensationTypes = await _repositoryWrapper.CompensationTypeRepository.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            if (compensationTypes == null)
                throw new NotFoundException("CompensationTypes not found");
            return _mapper.Map<List<CompensationTypeDto>>(compensationTypes);
        }

        public async Task CreateCompensationType(CompensationTypeDto compensationTypeDto)
        {
            if (compensationTypeDto == null)
                throw new NullReferenceException("CompensationTypeDto cannot be null");
            var compensationType = _mapper.Map<CompensationType>(compensationTypeDto);
            compensationType.CreateDate = DateTime.UtcNow;
            compensationType.CompensationTypeGuid = Guid.NewGuid();
            await _repositoryWrapper.CompensationTypeRepository.Create(compensationType);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdateCompensationType(Guid compensationTypeGuid, CompensationTypeDto compensationTypeDto)
        {
            if (compensationTypeDto == null || compensationTypeGuid == null || compensationTypeGuid == Guid.Empty)
                throw new NullReferenceException("CompensationTypeDto and CompensationTypeGuid cannot be null");
            var compensationType = await _repositoryWrapper.CompensationTypeRepository.GetByGuid(compensationTypeGuid);
            if (compensationType == null)
                throw new NotFoundException("CompensationType not found");
            compensationType.CompensationTypeName = compensationTypeDto.CompensationTypeName;
            compensationType.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteCompensationType(Guid compensationTypeGuid)
        {
            if (compensationTypeGuid == null || compensationTypeGuid == Guid.Empty)
                throw new NullReferenceException("CompensationTypeGuid cannot be null");
            var compensationType = await _repositoryWrapper.CompensationTypeRepository.GetByGuid(compensationTypeGuid);
            if (compensationType == null)
                throw new NotFoundException("CompensationType not found");
            compensationType.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
