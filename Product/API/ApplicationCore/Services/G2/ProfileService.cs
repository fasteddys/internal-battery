using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Services.G2
{
    public class ProfileService : IProfileService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IG2Service _g2Service;

        public ProfileService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IG2Service g2Service)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _g2Service = g2Service;
        }

        public async Task<Guid> CreateProfile(ProfileDto profileDto)
        {
            return await _repositoryWrapper.ProfileRepository.CreateProfile(profileDto);
        }

        public async Task DeleteProfile(Guid profileGuid)
        {
            await _repositoryWrapper.ProfileRepository.DeleteProfile(profileGuid);
        }

        public async Task<ProfileDto> GetProfileForRecruiter(Guid profileGuid, Guid subscriberGuid)
        {
            if (profileGuid == null || profileGuid == Guid.Empty)
                throw new NotFoundException("profileGuid cannot be null or empty");

            ProfileDto profileDto;
            var profile = await _repositoryWrapper.ProfileRepository.GetProfileForRecruiter(profileGuid, subscriberGuid);
            if (profile == null)
                throw new NotFoundException("profile not found");
            
            return _mapper.Map<ProfileDto>(profile);
        }

        public async Task UpdateProfileForRecruiter(ProfileDto profileDto, Guid subscriberGuid)
        {
            if (profileDto == null)
                throw new NotFoundException("profileDto cannot be null");
            await _repositoryWrapper.ProfileRepository.UpdateProfileForRecruiter(profileDto, subscriberGuid);

            // Update the profile in azure search            
            await _g2Service.IndexProfileAsync(profileDto.ProfileGuid);            
        }

        public async Task UpdateAzureIndexStatus(Guid profileGuid, string azureIndexStatusName, string azureSearchIndexInfo)
        {
            if (profileGuid == null || profileGuid == Guid.Empty)
                throw new NotFoundException("profileGuid cannot be null or empty");

            var azureIndexStatusQuery = await _repositoryWrapper.AzureIndexStatusRepository.GetByConditionAsync(ais => ais.Name == azureIndexStatusName);
            var azureIndexStatus = azureIndexStatusQuery.FirstOrDefault();
            if (azureIndexStatus == null)
                throw new NotFoundException($"unrecognized azure index status: {azureIndexStatusName}");

            await _repositoryWrapper.ProfileRepository.UpdateAzureIndexStatus(profileGuid, azureIndexStatus.AzureIndexStatusGuid, azureSearchIndexInfo);
        }
    }
}
