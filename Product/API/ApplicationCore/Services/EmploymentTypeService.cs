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
    public class EmploymentTypeService : IEmploymentTypeService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public EmploymentTypeService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<EmploymentTypeDto> GetEmploymentType(Guid employmentTypeGuid)
        {
            if (employmentTypeGuid == null || employmentTypeGuid == Guid.Empty)
                throw new NullReferenceException("EmploymentTypeGuid cannot be null");
            var employmentType = await  _repositoryWrapper.EmploymentTypeRepository.GetByGuid(employmentTypeGuid);
            if (employmentType == null)
                throw new NotFoundException($"EmploymentType with guid: {employmentTypeGuid} does not exist");
            return _mapper.Map<EmploymentTypeDto>(employmentType);
        }

        public async Task<EmploymentTypeListDto> GetEmploymentTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var employmentTypes = await _repositoryWrapper.StoredProcedureRepository.GetEmploymentTypes(limit, offset, sort, order);
            if (employmentTypes == null)
                throw new NotFoundException("EmploymentTypes not found");
            return _mapper.Map<EmploymentTypeListDto>(employmentTypes);
        }

        public async Task<Guid> CreateEmploymentType(EmploymentTypeDto employmentTypeDto)
        {
            if (employmentTypeDto == null)
                throw new NullReferenceException("EmploymentTypeDto cannot be null");
            var employmentType = _mapper.Map<EmploymentType>(employmentTypeDto);
            employmentType.CreateDate = DateTime.UtcNow;
            employmentType.EmploymentTypeGuid = Guid.NewGuid();
            await _repositoryWrapper.EmploymentTypeRepository.Create(employmentType);
            await _repositoryWrapper.SaveAsync();
            return employmentType.EmploymentTypeGuid;
        }

        public async Task UpdateEmploymentType(Guid employmentTypeGuid, EmploymentTypeDto employmentTypeDto)
        {
            if (employmentTypeDto == null || employmentTypeGuid == null || employmentTypeGuid == Guid.Empty)
                throw new NullReferenceException("EmploymentTypeDto and EmploymentTypeGuid cannot be null");
            var employmentType = await _repositoryWrapper.EmploymentTypeRepository.GetByGuid(employmentTypeGuid);
            if (employmentType == null)
                throw new NotFoundException("EmploymentType not found");
            employmentType.Name = employmentTypeDto.Name;
            employmentType.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteEmploymentType(Guid employmentTypeGuid)
        {
            if (employmentTypeGuid == null || employmentTypeGuid == Guid.Empty)
                throw new NullReferenceException("CountryGuid cannot be null");
            var employmentType = await _repositoryWrapper.EmploymentTypeRepository.GetByGuid(employmentTypeGuid);
            if (employmentType == null)
                throw new NotFoundException("EmploymentType not found");
            employmentType.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
