using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore.Extensions;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberRepository : UpDiddyRepositoryBase<Subscriber>, ISubscriberRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        private readonly ISubscriberGroupRepository _subscriberGroupRepository;
        private readonly IGroupPartnerRepository _groupPartnerRepository;
        private readonly IPartnerRepository _partnerRepository;
        public SubscriberRepository(UpDiddyDbContext dbContext, ISubscriberGroupRepository subscriberGroupRepository, IGroupPartnerRepository groupPartnerRepository, IPartnerRepository partnerRepository) : base(dbContext)
        {
            _subscriberGroupRepository = subscriberGroupRepository;
            _groupPartnerRepository = groupPartnerRepository;
            _partnerRepository = partnerRepository;
            _dbContext = dbContext;
        }

        public Task<IQueryable<Subscriber>> GetAllSubscribersAsync()
        {
            return GetAllAsync();
        }

        public async Task<Subscriber> GetSubscriberByEmailAsync(string email)
        {
            var subscriberResult =  _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.Email == email)
                              .FirstOrDefault();

            return subscriberResult;
        }  

        public async Task<Subscriber> GetSubscriberByGuidAsync(Guid subscriberGuid)
        {

   
            var subscriberResult = _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .FirstOrDefault();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberByIdAsync(int subscriberId)
        {
            return _dbContext.Subscriber
              .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
              .Include( s => s.State)
              .FirstOrDefault(); 
        }


        public async Task<IList<Partner>> GetPartnersAssociatedWithSubscriber(int subscriberId)
        {

            
            var subscriberGroups = _subscriberGroupRepository.GetAllAsync();
            var groupPartners = _groupPartnerRepository.GetAllAsync();
            var partners = _partnerRepository.GetAllAsync();

            return await subscriberGroups.Result
                .Join(groupPartners.Result, sg => sg.GroupId, gp => gp.GroupId, (sg, gp) => new { sg, gp })
                .Join(partners.Result, sg_gp => sg_gp.gp.PartnerId, partner => partner.PartnerId, (sg_gp, partner) => new Partner()
                {
                    ModifyDate = partner.ModifyDate,
                    LogoUrl = partner.LogoUrl,
                    IsDeleted = partner.IsDeleted,
                    ApiToken = partner.ApiToken,
                    CreateDate = partner.CreateDate,
                    CreateGuid = partner.CreateGuid,
                    Description = partner.Description,
                    ModifyGuid = partner.ModifyGuid,
                    Name = partner.Name,
                    PartnerGuid = partner.PartnerGuid,
                    PartnerId = partner.PartnerId,
                    PartnerType = partner.PartnerType,
                    PartnerTypeId = partner.PartnerTypeId,
                    Referrers = partner.Referrers,
                    WebRedirect = partner.WebRedirect                    
                })
                .ToListAsync<Partner>();
        
        }

    }
}
