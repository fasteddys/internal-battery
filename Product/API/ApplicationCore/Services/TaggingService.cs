
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

        public async Task AddSubscriberToGroupAsync(int GroupId, int SubscriberId)
        {
            SubscriberGroup subscriberGroup = await GetExistingSubscriberGroup(GroupId, SubscriberId);
            if (subscriberGroup != null)
                return ;

            DateTime currentDateTime = DateTime.UtcNow;
            subscriberGroup = new SubscriberGroup
            {
                CreateDate = currentDateTime,
                GroupId = GroupId,
                SubscriberId = SubscriberId,
                ModifyDate = currentDateTime,
                SubscriberGroupGuid = Guid.NewGuid()
            };
            await _repositoryWrapper.SubscriberGroupRepository.CreateSubscriberGroup(subscriberGroup);
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
                await AddSubscriberToGroupAsync(groupPartner.GroupId, SubscriberId);
            }

            return true;
        }

        public async Task AssignGroup(Guid GroupGuid, int SubscriberId)
        {
            //get group related to groupGuid
            Group Group = await GetGroupBasedOnGuid(GroupGuid);

            //Assign Subscriber to group
            await AddSubscriberToGroupAsync(Group.GroupId, SubscriberId);
        }



        #region Helpers

        private async Task<Group> GetGroupBasedOnGuid(Guid GroupGuid)
        {
            Group group = await _repositoryWrapper.GroupRepository.GetGroupByGuid(GroupGuid);

            //if the group is null get the default CareerCircle Group
            if(group == null)
                group = await _repositoryWrapper.GroupRepository.GetDefaultGroup();

            return group;
        }

        private async Task<SubscriberGroup> GetExistingSubscriberGroup(int GroupId, int SubscriberId)
        {
            SubscriberGroup subscriberGroup = await _repositoryWrapper.SubscriberGroupRepository.GetSubscriberGroupByGroupIdSubscriberId(GroupId, SubscriberId);

            return subscriberGroup;
        }

        #endregion

    }
}
