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
            IList<EmploymentTypeDto> rval;
            var employmentTypes = await _repositoryWrapper.EmploymentTypeRepository.GetAllEmploymentTypes();
            if (employmentTypes == null)
                throw new NotFoundException("EmploymentTypeGuid not found");
            rval = _mapper.Map<List<EmploymentTypeDto>>(employmentTypes);
            return rval?.Where(x => x.EmploymentTypeGuid == employmentTypeGuid).FirstOrDefault();
        }

        public async Task<List<EmploymentTypeDto>> GetEmploymentTypes(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var employmentTypes = await _repositoryWrapper.EmploymentTypeRepository.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            if (employmentTypes == null)
                throw new NotFoundException("EmploymentTypes not found");
            return _mapper.Map<List<EmploymentTypeDto>>(employmentTypes);
        }

        public async Task CreateEmploymentType(EmploymentTypeDto employmentTypeDto)
        {
            if (employmentTypeDto == null)
                throw new NullReferenceException("EmploymentTypeDto cannot be null");
            var employmentType = _mapper.Map<EmploymentType>(employmentTypeDto);
            employmentType.CreateDate = DateTime.UtcNow;
            employmentType.EmploymentTypeGuid = Guid.NewGuid();
            await _repositoryWrapper.EmploymentTypeRepository.Create(employmentType);
            await _repositoryWrapper.SaveAsync();
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
