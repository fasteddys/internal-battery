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
    public class IndustryService : IIndustryService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public IndustryService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<IndustryDto> GetIndustry(Guid industryGuid)
        {
            if (industryGuid == null || industryGuid == Guid.Empty)
                throw new NullReferenceException("IndustryGuid cannot be null");
            IList<IndustryDto> rval;
            var industry = await _repositoryWrapper.IndustryRepository.GetByGuid(industryGuid);
            if (industry == null)
                throw new NotFoundException("Industry not found");
            return _mapper.Map<IndustryDto>(industry);
        }

        public async Task<IndustryListDto> GetIndustries(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var industrys = await _repositoryWrapper.StoredProcedureRepository.GetIndustries(limit, offset, sort, order);
            if (industrys == null)
                throw new NotFoundException("Industrys not found");
            return _mapper.Map<IndustryListDto>(industrys);
        }

        public async Task<Guid> CreateIndustry(IndustryDto industryDto)
        {
            if (industryDto == null)
                throw new NullReferenceException("IndustryDto cannot be null");
            var industry = _mapper.Map<Industry>(industryDto);
            industry.CreateDate = DateTime.UtcNow;
            industry.IndustryGuid = Guid.NewGuid();
            await _repositoryWrapper.IndustryRepository.Create(industry);
            await _repositoryWrapper.SaveAsync();
            return industry.IndustryGuid;
        }

        public async Task UpdateIndustry(Guid industryGuid, IndustryDto industryDto)
        {
            if (industryDto == null || industryGuid == null || industryGuid == Guid.Empty)
                throw new NullReferenceException("IndustryDto and IndustryGuid cannot be null");
            var industry = await _repositoryWrapper.IndustryRepository.GetByGuid(industryGuid);
            if (industry == null)
                throw new NotFoundException("Industry not found");
            industry.Name = industryDto.Name;
            industry.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteIndustry(Guid industryGuid)
        {
            if (industryGuid == null || industryGuid == Guid.Empty)
                throw new NullReferenceException("industryGuid cannot be null");
            var industry = await _repositoryWrapper.IndustryRepository.GetByGuid(industryGuid);
            if (industry == null)
                throw new NotFoundException("Industry not found");
            industry.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
