using System;

using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using AutoMapper;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class CompensationTypeService : ICompensationTypeService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public CompensationTypeService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<CompensationTypeDto> GetCompensationType(Guid compensationTypeGuid)
        {
            if (compensationTypeGuid == null || compensationTypeGuid == Guid.Empty)
                throw new NullReferenceException("CompensationTypeGuid cannot be null");
            var compensationType = await _repositoryWrapper.CompensationTypeRepository.GetByGuid(compensationTypeGuid);
            if( compensationType == null)
                throw new NotFoundException($"CompensationType with guid: {compensationTypeGuid} does not exist");
            return _mapper.Map<CompensationTypeDto>(compensationType);
        }

        public async Task<CompensationTypeListDto> GetCompensationTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var compensationTypes = await _repositoryWrapper.StoredProcedureRepository.GetCompensationTypes(limit, offset, sort, order);
            if (compensationTypes == null)
                throw new NotFoundException("CompensationTypes not found");
            return _mapper.Map<CompensationTypeListDto>(compensationTypes);
        }

        public async Task<Guid> CreateCompensationType(CompensationTypeDto compensationTypeDto)
        {
            if (compensationTypeDto == null)
                throw new NullReferenceException("CompensationTypeDto cannot be null");
            var compensationType = _mapper.Map<CompensationType>(compensationTypeDto);
            compensationType.CreateDate = DateTime.UtcNow;
            compensationType.CompensationTypeGuid = Guid.NewGuid();
            await _repositoryWrapper.CompensationTypeRepository.Create(compensationType);
            await _repositoryWrapper.SaveAsync();
            return compensationType.CompensationTypeGuid;
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
