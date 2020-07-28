using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using Microsoft.Extensions.Configuration;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class OfferService : IOfferService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IHangfireService _hangfireService;
        private readonly IMapper _mapper;
        private ISysEmail _sysEmail;
        private readonly IConfiguration _configuration;

        public OfferService(IHangfireService hangfireService
        , IMapper mapper
        , IRepositoryWrapper repositoryWrapper
        , IConfiguration configuration
        , ISysEmail sysEmail)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _configuration = configuration;
            _sysEmail = sysEmail;
            _hangfireService = hangfireService;
        }

        public async Task<OfferListDto> GetOffers(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var offers = await _repositoryWrapper.StoredProcedureRepository.GetOffers(limit, offset, sort, order);
            return _mapper.Map<OfferListDto>(offers);
        }

        public async Task<OfferDto> GetOffer(Guid offerGuid)
        {
            var offer = await _repositoryWrapper.Offer.GetOfferByGuid(offerGuid);
            if (offer == null)
                throw new NotFoundException("Offer not found.");
            return _mapper.Map<OfferDto>(offer);
        }

        public async Task<Guid> ClaimOffer(Guid subscriberGuid, Guid offerGuid)
        {
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found.");

            var subscriberFile = await _repositoryWrapper.SubscriberFileRepository.GetAllSubscriberFilesBySubscriberGuid(subscriberGuid);
            if (subscriberFile.Count == 0)
                throw new OfferException("Subscriber is not eligible for claiming offer due to missing resume");

            var offer = await _repositoryWrapper.Offer.GetByGuid(offerGuid);
            if (offer == null)
                throw new NotFoundException("Offer not found.");

            var action = await GetAction();

            var entityType = await GetEntityType();
            Guid subscriberActionGuid = Guid.NewGuid();
            await _repositoryWrapper.SubscriberActionRepository.Create(
                 new SubscriberAction()
                 {
                     SubscriberActionGuid = subscriberActionGuid,
                     CreateDate = DateTime.UtcNow,
                     CreateGuid = Guid.Empty,
                     ActionId = action.ActionId,
                     EntityId = offer.OfferId,
                     EntityTypeId = entityType == null ? null : (int?)entityType.EntityTypeId,
                     IsDeleted = 0,
                     OccurredDate = DateTime.UtcNow,
                     SubscriberId = subscriber.SubscriberId
                 });
            await _repositoryWrapper.SaveAsync();

            _hangfireService.Enqueue(() =>
                _sysEmail.SendTemplatedEmailAsync( subscriber.Email, _configuration["SysEmail:Transactional:TemplateIds:SubscriberOffer-Redemption"], offer, Constants.SendGridAccount.Transactional, null, null, null, null, null, null));

            return subscriberActionGuid;
        }

        public async Task<bool> HasSubscriberClaimedOffer(Guid subscriberGuid, Guid offerGuid)
        {
            var offer = await _repositoryWrapper.Offer.GetByGuid(offerGuid);
            if (offer == null)
                throw new NotFoundException("Offer not found.");
            var action = await GetAction();
            var entityType = await GetEntityType();
            var ClaimedOffers = await _repositoryWrapper.SubscriberActionRepository.GetSubscriberActionByEntityAndEntityTypeAndAction(entityType.EntityTypeId, offer.OfferId, action.ActionId);
            if (ClaimedOffers.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Guid> CreateOffer(OfferDto offerDto)
        {
            if (offerDto == null)
                throw new NullReferenceException("OfferDto cannot be null.");
            if (offerDto.PartnerGuid == null)
                throw new NullReferenceException("PartnerGuid cannot be null.");
            var partner = await _repositoryWrapper.PartnerRepository.GetByGuid(offerDto.PartnerGuid.Value);
            if (partner == null)
                throw new NotFoundException("Partner not found");
            var offer = _mapper.Map<Offer>(offerDto);
            offer.PartnerId = partner.PartnerId;
            offer.OfferGuid = Guid.NewGuid();
            await _repositoryWrapper.Offer.Create(offer);
            await _repositoryWrapper.SaveAsync();
            return offer.OfferGuid;
        }


        public async Task UpdateOffer(Guid offerGuid, OfferDto offerDto)
        {
            var offer = await _repositoryWrapper.Offer.GetByGuid(offerGuid);
            if (offer == null)
                throw new NotFoundException("Offer not found.");
            offer.Description = offerDto.Description;
            offer.Disclaimer = offerDto.Disclaimer;
            offer.Name = offerDto.Name;
            offer.StartDate = offerDto.startDate;
            offer.EndDate = offerDto.endDate;
            offer.Url = offerDto.Url;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteOffer(Guid offerGuid)
        {
            var offer = await _repositoryWrapper.Offer.GetByGuid(offerGuid);
            if (offer == null)
                throw new NotFoundException("Offer not found.");
            offer.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }

        private async Task<UpDiddyApi.Models.Action> GetAction()
        {
            var action = await _repositoryWrapper.ActionRepository.GetByNameAsync("Partner offer");
            if (action == null)
                throw new InvalidOperationException("Action not found");
            return action;
        }

        private async Task<UpDiddyApi.Models.EntityType> GetEntityType()
        {
            var entityType = await _repositoryWrapper.EntityTypeRepository.GetByNameAsync("Offer");
            if (entityType == null)
                throw new InvalidOperationException("EntityType not found");
            return entityType;
        }
    }
}
