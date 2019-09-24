
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


        public async Task<bool> AssociateSourceToSubscriber(string Source, int SubscriberId)
        {
            // TODO JAB remove try catch 
            try
            {
                // short circuit if subscriber is already associated with the partner
                IList<Partner> Partners = await _repositoryWrapper.PartnerContactRepository.GetPartnersAssociatedWithSubscriber(SubscriberId);
                if (Partners != null)
                {
                    foreach (Partner p in Partners)
                        if (p.Name == Source)
                            return true;
                }


                PartnerType partnerType = await _repositoryWrapper.PartnerTypeRepository.GetPartnerTypeByName("ExternalSource");

                if (partnerType == null)
                    return false;


                // Find or create  the source as a partner 
                Partner Partner = await _repositoryWrapper.PartnerRepository.GetPartnerByName(Source);
                if (Partner == null)
                {
                    Partner = new Partner()
                    {
                        CreateDate = DateTime.UtcNow,
                        ModifyDate = DateTime.UtcNow,
                        CreateGuid = Guid.NewGuid(),
                        ModifyGuid = Guid.NewGuid(),
                        PartnerGuid = Guid.NewGuid(),                         
                        IsDeleted = 0,
                        Name = Source,
                        PartnerTypeId =    partnerType.PartnerTypeId,
                         
                    };
                    await _repositoryWrapper.PartnerRepository.Create(Partner);
                    await _repositoryWrapper.PartnerRepository.SaveAsync();
                }

                // Create/Find group and add user to it
                await CreateGroup(Source, Partner.PartnerGuid.Value, SubscriberId);
            }
            catch ( Exception ex )
            {
                var x = ex.Message;
            }
            
            return true;

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
            await _repositoryWrapper.SubscriberGroupRepository.Create(subscriberGroup);
            await _repositoryWrapper.SubscriberGroupRepository.SaveAsync();

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
                await AddSubscriberToGroupAsync(groupPartner.GroupId, SubscriberId);
            }

            return true;
        }

        public async Task<bool> CreateGroup(string ReferrerUrl, Guid PartnerGuid, int SubscriberId)
        {
            try
            {
                Group Group = await GetGroupBasedOnReferrerUrl(ReferrerUrl);
                await AddSubscriberToGroupAsync(Group.GroupId, SubscriberId);
                if (PartnerGuid != null && PartnerGuid != Guid.Empty)
                {
                    IEnumerable<Partner> iePartner = await _repositoryWrapper.PartnerRepository.GetByConditionAsync(p => p.PartnerGuid == PartnerGuid);
                    Partner Partner = iePartner.FirstOrDefault();

                    GroupPartner existingGroupPartner=await _repositoryWrapper.GroupPartnerRepository.GetGroupPartnerByGroupIdPartnerIdAsync(Group.GroupId, Partner.PartnerId);

                    if(existingGroupPartner==null)
                    {
                        DateTime CurrentDateTime = DateTime.UtcNow;

                        GroupPartner GroupPartner = new GroupPartner
                        {
                            CreateDate = CurrentDateTime,
                            CreateGuid = Guid.Empty,
                            GroupId = Group.GroupId,
                            GroupPartnerGuid = Guid.NewGuid(),
                            ModifyDate = CurrentDateTime,
                            PartnerId = Partner.PartnerId
                        };

                        await _repositoryWrapper.GroupPartnerRepository.Create(GroupPartner);
                        await _repositoryWrapper.GroupPartnerRepository.SaveAsync();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"TaggingService:_CreateGroup threw an exception -> {e.Message} for subscriber {SubscriberId} PartnerGuid {PartnerGuid} ReferralUrl {ReferrerUrl}");
                return false;
            }
        }



        #region Helpers

        private async Task<Group> GetGroupBasedOnReferrerUrl(string ReferrerUrl)
        {
            IEnumerable<Group> ieGroup = await _repositoryWrapper.GroupRepository.GetByConditionAsync(g => g.Path.Equals(ReferrerUrl));
            Group groupBasedOnReferrerUrl = ieGroup.FirstOrDefault();

            if(groupBasedOnReferrerUrl == null)
                groupBasedOnReferrerUrl = await _repositoryWrapper.GroupRepository.CreateAutoGeneratedGroup(ReferrerUrl);

            return groupBasedOnReferrerUrl;
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
