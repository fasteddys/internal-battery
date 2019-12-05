using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TalentFavoriteService : ITalentFavoriteService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public TalentFavoriteService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration configuration)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = configuration;
        }

        public async Task AddToFavorite(Guid subscriberGuid, Guid talentGuid)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            if (talentGuid == null || talentGuid == Guid.Empty)
                throw new NullReferenceException("TalentGuid cannot be null");

            var talent = await _repositoryWrapper.SubscriberRepository.GetByGuid(talentGuid);
            if (talent == null)
                throw new NotFoundException("Talent not found");
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");

            var talentFavorite = await _repositoryWrapper.TalentFavoriteRepository.GetBySubscriberGuidAndTalentGuid(subscriberGuid, talentGuid);
            if (talentFavorite == null)
            {
                talentFavorite = new TalentFavorite()
                {

                    Talent = talent,
                    TalentId = talent.SubscriberId,
                    CreateDate = DateTime.UtcNow,
                    SubscriberId = subscriber.SubscriberId,
                    CreateGuid = subscriberGuid,
                    TalentFavoriteGuid = Guid.NewGuid(),
                    IsDeleted = 0
                    
                };
                await _repositoryWrapper.TalentFavoriteRepository.Create(talentFavorite);
                await _repositoryWrapper.SaveAsync();
            }
            else
            {
                if (talentFavorite.IsDeleted == 0)
                {
                    throw new AlreadyExistsException("Talent already added to favorites");
                }
                else
                {
                    talentFavorite.IsDeleted = 0;
                    talentFavorite.ModifyDate = DateTime.UtcNow;
                    await _repositoryWrapper.SaveAsync();
                }
            }
        }
 
        public async Task RemoveFromFavorite(Guid subscriberGuid, Guid talentGuid)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NullReferenceException("SubscriberGuid cannot be null");
            if (talentGuid == null || talentGuid == Guid.Empty)
                throw new NullReferenceException("Talent cannot be null");
            var talent = await _repositoryWrapper.SubscriberRepository.GetByGuid(talentGuid);
            if (talent == null)
                throw new NotFoundException("Talent not found");
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");

            var talentFavorite = await _repositoryWrapper.TalentFavoriteRepository.GetBySubscriberGuidAndTalentGuid(subscriberGuid, talentGuid);
            if (talentFavorite == null)
            {
                throw new NotFoundException("Talent has not been added to favorites");
            }
            else
            {
                talentFavorite.IsDeleted = 1;
                talentFavorite.ModifyDate = DateTime.UtcNow;
                await _repositoryWrapper.SaveAsync();
            }
        }
 
 
        public async Task<List<TalentFavoriteDto>> GetFavoriteTalent(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var talent = await _repositoryWrapper.TalentFavoriteRepository.GetTalentForSubscriber(subscriberGuid);
            if (talent == null)
                throw new NotFoundException("Talent not found");
            return ( _mapper.Map<List<TalentFavoriteDto>>(talent) );
        }
    }
}
