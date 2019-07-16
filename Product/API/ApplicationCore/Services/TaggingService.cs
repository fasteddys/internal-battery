
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

        public async Task<bool> AddSubscriberToGroupAsync(int GroupId, int SubscriberId)
        {
            SubscriberGroup subscriberGroup = await GetExistingSubscriberGroup(GroupId, SubscriberId);
            if (subscriberGroup != null)
                return false;

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

            return true;
        }

        public async Task<bool> AddSubscriberToGroupBasedOnReferrerUrlAsync(int SubscriberId, string ReferrerUrl)
        {
            // Get all urls that match the given referer url. Allows for more than one.
            IEnumerable<PartnerReferrer> iePartnerReferrers = await _repositoryWrapper.PartnerReferrerRepository
                        .GetByConditionAsync(pr => pr.Path.Equals(ReferrerUrl));

            IList<PartnerReferrer> partnerReferrers = iePartnerReferrers.ToList();

            if(partnerReferrers.Count == 0)
            {
                // If no link is found to a partner, simply add a generated group
                Group Group = await GenerateGroup(ReferrerUrl);
                AddSubscriberToGroupAsync(Group.GroupId, SubscriberId);
            }
            else
            {
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
            

            return true;
        }

        public async Task<bool> AddConvertedContactToGroupBasedOnPartnerAsync(int SubscriberId)
        {
            IList<Partner> Partners = await _repositoryWrapper.PartnerContactRepository.GetPartnersAssociatedWithSubscriber(SubscriberId);

            if (Partners == null)
                return false;

            IList<GroupPartner> GroupPartners = new List<GroupPartner>();
            foreach(Partner partner in Partners)
            {
                GroupPartners.Add(_repositoryWrapper.GroupPartnerRepository.GetByConditionAsync(gp => gp.PartnerId == partner.PartnerId).Result.FirstOrDefault());
            }

            foreach(GroupPartner groupPartner in GroupPartners)
            {
                AddSubscriberToGroupAsync(groupPartner.GroupId, SubscriberId);
            }

            return true;
        }

        public async Task<bool> EnsurePartnerReferrerEntryExistsIfPartnerSpecified(string ReferrerUrl, Guid PartnerGuid, int SubscriberId)
        {
            IEnumerable<Partner> iePartner = await _repositoryWrapper.PartnerRepository.GetByConditionAsync(p => p.PartnerGuid == PartnerGuid);
            Partner Partner = iePartner.FirstOrDefault();
            IEnumerable<PartnerReferrer> iePartnerReferrer = await _repositoryWrapper.PartnerReferrerRepository
                .GetByConditionAsync(pr => pr.PartnerId == Partner.PartnerId && pr.Path.Equals(ReferrerUrl));
            PartnerReferrer PartnerReferrer = iePartnerReferrer.FirstOrDefault();

            if (PartnerReferrer != null)
                return false;

            PartnerReferrer NewPartnerReferrer = new PartnerReferrer
            {
                PartnerId = Partner.PartnerId,
                Path = ReferrerUrl
            };

            _repositoryWrapper.PartnerReferrerRepository.Create(NewPartnerReferrer);
            await _repositoryWrapper.PartnerReferrerRepository.SaveAsync();

            DateTime CurrentDateTime = DateTime.UtcNow;

            Group Group = await GenerateGroup(ReferrerUrl);

            GroupPartner GroupPartner = new GroupPartner
            {
                CreateDate = CurrentDateTime,
                CreateGuid = Guid.Empty,
                GroupId = Group.GroupId,
                GroupPartnerGuid = Guid.NewGuid(),
                ModifyDate = CurrentDateTime,
                PartnerId = Partner.PartnerId
            };

            _repositoryWrapper.GroupPartnerRepository.Create(GroupPartner);
            await _repositoryWrapper.GroupPartnerRepository.SaveAsync();

            AddSubscriberToGroupAsync(Group.GroupId, SubscriberId);

            return true;
        }

        #region Helpers

        private async Task<Group> GenerateGroup(string ReferrerUrl)
        {
            return await _repositoryWrapper.GroupRepository.CreateAutoGeneratedGroup(ReferrerUrl);
        }

        private async Task<SubscriberGroup> GetExistingSubscriberGroup(int GroupId, int SubscriberId)
        {
            IEnumerable<SubscriberGroup> subscriberGroup = await _repositoryWrapper.SubscriberGroupRepository.GetByConditionAsync(sg => 
                sg.GroupId == GroupId && 
                sg.SubscriberId == SubscriberId && 
                sg.IsDeleted == 0);

            return subscriberGroup.FirstOrDefault();

        }

        #endregion

    }
}
