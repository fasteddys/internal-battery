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
    public class PartnerService : IPartnerService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public PartnerService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<PartnerDto> GetPartner(Guid partnerGuid)
        {
            if (partnerGuid == null || partnerGuid == Guid.Empty)
                throw new NullReferenceException("PartnerGuid cannot be null");
            var partner = await _repositoryWrapper.PartnerRepository.GetByGuid(partnerGuid);
            if(partner == null)
                throw new NotFoundException($"Parnter with the guid: {partnerGuid} not found.");
            return _mapper.Map<PartnerDto>(partner);
        }

        public async Task<PartnerListDto> GetPartners(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var partners = await _repositoryWrapper.StoredProcedureRepository.GetPartners(limit, offset, sort, order);
            if (partners == null)
                throw new NotFoundException("Partners not found");
            return _mapper.Map<PartnerListDto>(partners);
        }

        public async Task CreatePartner(PartnerDto partnerDto)
        {
            if (partnerDto == null)
                throw new NullReferenceException("PartnerDto cannot be null");
            var partner = new Partner();
            partner.PartnerGuid = Guid.NewGuid();
            partner.Name = partnerDto.Name;
            partner.Description = partnerDto.Description;
            await _repositoryWrapper.PartnerRepository.Create(partner);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdatePartner(Guid partnerGuid, PartnerDto partnerDto)
        {
            if (partnerDto == null || partnerGuid == null || partnerGuid == Guid.Empty)
                throw new NullReferenceException("PartnerDto and PartnerGuid cannot be null");
            var partner = await _repositoryWrapper.PartnerRepository.GetByGuid(partnerGuid);
            if (partner == null)
                throw new NotFoundException("Partner not found");
            partner.Name = partnerDto.Name;
            partner.Description = partnerDto.Description;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeletePartner(Guid partnerGuid)
        {
            if (partnerGuid == null || partnerGuid == Guid.Empty)
                throw new NullReferenceException("PartnerGuid cannot be null");
            var partner = await _repositoryWrapper.PartnerRepository.GetByGuid(partnerGuid);
            if (partner == null)
                throw new NotFoundException("Partner not found");
            partner.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
