
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TaggingService : ITaggingService
    {
        private IConfiguration _configuration { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repositoryWrapper { get; set; }
        private readonly IMapper _mapper;

        public TaggingService(
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper)
        {
            _configuration = configuration;
            _repositoryWrapper = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async void AddSubscriberToGroupAsync(int GroupId, int SubscriberId)
        {
            SubscriberGroup subscriberGroup = GetExistingSubscriberGroup(GroupId, SubscriberId);
            if (subscriberGroup != null)
                return;

            DateTime currentDateTime = DateTime.UtcNow;
            subscriberGroup = new SubscriberGroup
            {
                CreateDate = currentDateTime,
                GroupId = GroupId,
                SubscriberId = SubscriberId,
                ModifyDate = currentDateTime,
                SubscriberGroupGuid = Guid.NewGuid()
            };
            _repositoryWrapper.SubscriberGroupRepository.Create(subscriberGroup);
            await _repositoryWrapper.SubscriberGroupRepository.SaveAsync();
        }

        public async void AddSubscriberToGroupBasedOnReferrerUrlAsync(int SubscriberId, string ReferrerUrl)
        {
            // Get all urls that match the given referer url. Allows for more than one.
            IList<PartnerReferrer> partnerReferrers = _repositoryWrapper.PartnerReferrerRepository
                        .GetByConditionAsync(pr => pr.Path.Equals(ReferrerUrl)).Result.ToList();

            // Iterate through each referer url. Will almost always just be one.
            foreach (PartnerReferrer partnerReferrer in partnerReferrers)
            {
                IList<GroupPartner> groupPartners = _repositoryWrapper.GroupPartnerRepository
                    .GetByConditionAsync(gp => gp.PartnerId == partnerReferrer.PartnerId).Result.ToList();

                // Iterate through all groups associated with the partner of the referer url.
                foreach (GroupPartner groupPartner in groupPartners)
                {
                    AddSubscriberToGroupAsync(groupPartner.GroupId, SubscriberId);
                }
            }
        }

        public async void AddConvertedContactToGroupBasedOnPartnerAsync(int SubscriberId)
        {
            IList<Partner> Partners = await _repositoryWrapper.PartnerContactRepository.GetPartnersAssociatedWithSubscriber(SubscriberId);

            if (Partners == null)
                return;

            IList<GroupPartner> GroupPartners = new List<GroupPartner>();
            foreach(Partner partner in Partners)
            {
                GroupPartners.Add(_repositoryWrapper.GroupPartnerRepository.GetByConditionAsync(gp => gp.PartnerId == partner.PartnerId).Result.FirstOrDefault());
            }

            foreach(GroupPartner groupPartner in GroupPartners)
            {
                AddSubscriberToGroupAsync(groupPartner.GroupId, SubscriberId);
            }
        }

        #region Helpers

        private SubscriberGroup GetExistingSubscriberGroup(int GroupId, int SubscriberId)
        {
            SubscriberGroup subscriberGroup = _repositoryWrapper.SubscriberGroupRepository.GetByConditionAsync(sg => 
                sg.GroupId == GroupId && 
                sg.SubscriberId == SubscriberId && 
                sg.IsDeleted == 0).Result.FirstOrDefault();

            return subscriberGroup;

        }

        #endregion

    }
}
