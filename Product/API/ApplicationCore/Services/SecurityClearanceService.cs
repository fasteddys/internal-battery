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
    public class SecurityClearanceService : ISecurityClearanceService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public SecurityClearanceService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<SecurityClearanceDto> GetSecurityClearance(Guid securityClearanceGuid)
        {
            if (securityClearanceGuid == null || securityClearanceGuid == Guid.Empty)
                throw new NullReferenceException("SecurityClearanceGuid cannot be null");

            IList<SecurityClearanceDto> rval;
            var securityClearances = await _repositoryWrapper.SecurityClearanceRepository.GetAllSecurityClearances();
            if (securityClearances == null)
                throw new NotFoundException("SecurityClearanceGuid not found");
            rval = _mapper.Map<List<SecurityClearanceDto>>(securityClearances);
            return rval?.Where(x => x.SecurityClearanceGuid == securityClearanceGuid).FirstOrDefault();
        }

        public async Task<List<SecurityClearanceDto>> GetSecurityClearances(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var securityClearances = await _repositoryWrapper.SecurityClearanceRepository.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            if (securityClearances == null)
                throw new NotFoundException("SecurityClearances not found");
            return _mapper.Map<List<SecurityClearanceDto>>(securityClearances);
        }

        public async Task CreateSecurityClearance(SecurityClearanceDto securityClearanceDto)
        {
            if (securityClearanceDto == null)
                throw new NullReferenceException("SecurityClearanceDto cannot be null");
            var securityClearance = _mapper.Map<SecurityClearance>(securityClearanceDto);
            securityClearance.CreateDate = DateTime.UtcNow;
            securityClearance.SecurityClearanceGuid = Guid.NewGuid();
            await _repositoryWrapper.SecurityClearanceRepository.Create(securityClearance);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdateSecurityClearance(Guid securityClearanceGuid, SecurityClearanceDto securityClearanceDto)
        {
            if (securityClearanceDto == null || securityClearanceGuid == null || securityClearanceGuid == Guid.Empty)
                throw new NullReferenceException("SecurityClearanceDto and SecurityClearanceGuid cannot be null");
            var securityClearance = await _repositoryWrapper.SecurityClearanceRepository.GetByGuid(securityClearanceGuid);
            if (securityClearance == null)
                throw new NotFoundException("SecurityClearance not found");
            securityClearance.Name = securityClearanceDto.Name;
            securityClearance.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteSecurityClearance(Guid securityClearanceGuid)
        {
            if (securityClearanceGuid == null || securityClearanceGuid == Guid.Empty)
                throw new NullReferenceException("SecurityClearanceGuid cannot be null");
            var securityClearance = await _repositoryWrapper.SecurityClearanceRepository.GetByGuid(securityClearanceGuid);
            if (securityClearance == null)
                throw new NotFoundException("SecurityClearance not found");
            securityClearance.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
