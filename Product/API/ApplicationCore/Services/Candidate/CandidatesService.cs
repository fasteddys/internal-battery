using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models.Candidate360;

namespace UpDiddyApi.ApplicationCore.Services.Candidate
{
    public class CandidatesService : ICandidatesService
    {
        private readonly IRepositoryWrapper _repository;
        private readonly ILogger _logger;

        public CandidatesService(
            IRepositoryWrapper repository,
            ILogger<CandidatesService> logger
            )
        {
            _repository = repository;
            _logger = logger;
        }

        // This empty service class will be used for stories:
        //   #2480 - Candidate 360: Personal Info
        //   #2481 - Candidate 360: Employment Preferences
        //   #2482 - Candidate 360: Role Preferences

        #region Personal Info

        #endregion Personal Info

        #region Employment Preferences

        #endregion Employment Preferences

        #region Role Preferences

        public async Task<Candidate360RoleDto> GetCandidate360Role(Guid subscriberGuid)
        {
            try
            {
                _logger.LogDebug("CandidatesService:GetCandidate360Role: Fetching Candidate 360 Role information for {subscriber}", subscriberGuid);

                var candidate360Role = await _repository.SubscriberRepository.GetCandidate360Role(subscriberGuid);
                _logger.LogDebug("CandidatesService:GetCandidate360Role: Returning Candidate 360 Role information for {subscriber}", subscriberGuid);

                return candidate360Role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:GetCandidate360Role: Error while fetching Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        public async Task UpdateCandidate360Role(Guid subscriberGuid, Candidate360RoleDto candidate360Role)
        {
            try
            {
                _logger.LogDebug("CandidatesService:GetCandidate360Role: Updating Candidate 360 Role information for {subscriber}", subscriberGuid);

                await _repository.SubscriberRepository.UpdateCandidate360Role(subscriberGuid, candidate360Role);
                _logger.LogDebug("CandidatesService:GetCandidate360Role: Updated Candidate 360 Role information for {subscriber}", subscriberGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:GetCandidate360Role: Error while updating Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        #endregion Role Preferences
    }
}
