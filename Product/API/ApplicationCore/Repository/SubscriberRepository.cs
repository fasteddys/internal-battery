using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Domain.Models;
using Microsoft.EntityFrameworkCore.Extensions;
 

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberRepository : UpDiddyRepositoryBase<Subscriber>, ISubscriberRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        private readonly ISubscriberGroupRepository _subscriberGroupRepository;
        private readonly IGroupPartnerRepository _groupPartnerRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IStoredProcedureRepository _storedProcedureRepository;

        public SubscriberRepository(UpDiddyDbContext dbContext, ISubscriberGroupRepository subscriberGroupRepository, IGroupPartnerRepository groupPartnerRepository, IPartnerRepository partnerRepository, IStoredProcedureRepository storedProcedureRepository) : base(dbContext)
        {
            _subscriberGroupRepository = subscriberGroupRepository;
            _groupPartnerRepository = groupPartnerRepository;
            _partnerRepository = partnerRepository;
            _storedProcedureRepository = storedProcedureRepository;
            _dbContext = dbContext;
        }

        public IQueryable<Subscriber> GetAllSubscribersAsync()
        {
            return GetAll();
        }

        public async Task<Subscriber> GetSubscriberByEmailAsync(string email)
        {
            var subscriberResult =  await _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.Email == email)
                              .FirstOrDefaultAsync();

            return subscriberResult;
        }

        // get the parter attributed with source attribution (i.e. caused the subscriber to join cc )
        public async Task<SubscriberSourceDto> GetSubscriberSource(int subscriberId)
        {
            var sources = await _storedProcedureRepository.GetSubscriberSources(subscriberId);
            return sources
                .Where(s => s.PartnerRank == 1 && s.GroupRank == 1)
                .FirstOrDefault();
        }


        public Subscriber GetSubscriberByEmail(string email)
        {
            var subscriberResult =  _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.Email == email)
                              .FirstOrDefault();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberByGuidAsync(Guid subscriberGuid)
        {

   
            var subscriberResult = await _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .FirstOrDefaultAsync();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberByIdAsync(int subscriberId)
        {
            return await _dbContext.Subscriber
              .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
              .Include( s => s.State)
              .FirstOrDefaultAsync(); 
        }

        public async Task<List<SubscriberEmploymentTypes>> GetCandidateEmploymentPreferencesBySubscriberGuidAsync(Guid subscriberGuid)
        {
            var subscriberEmploymentTypes = await _dbContext.SubscriberEmploymentTypes
                              .Where(s => s.Subscriber.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid)
                              .Include(s => s.EmploymentType)
                              .Include(s => s.Subscriber.CommuteDistance)
                              .ToListAsync();

            return subscriberEmploymentTypes;
        }

        public Subscriber GetSubscriberByGuid(Guid subscriberGuid)
        {


            var subscriberResult =  _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .FirstOrDefault();

            return subscriberResult;
        }

        public async Task<IList<Partner>> GetPartnersAssociatedWithSubscriber(int subscriberId)
        {


            var subscriberGroups = _subscriberGroupRepository.GetAll()
                .Where(s => s.SubscriberId == subscriberId);
            
            var groupPartners = _groupPartnerRepository.GetAll();                
            var partners = _partnerRepository.GetAll();

            return await subscriberGroups
                .Join(groupPartners, sg => sg.GroupId, gp => gp.GroupId, (sg, gp) => new { sg, gp })
                .Join(partners, sg_gp => sg_gp.gp.PartnerId, partner => partner.PartnerId, (sg_gp, partner) => new Partner()
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

        public async Task<int> GetSubscribersCountByStartEndDates(DateTime? startDate = null, DateTime? endDate = null)
        {
            //get queryable object for subscribers
            var queryableSubscribers = GetAll();

            if (startDate.HasValue)
            {
                queryableSubscribers = queryableSubscribers.Where(s => s.CreateDate >= startDate);
            }
            if (endDate.HasValue)
            {
                queryableSubscribers = queryableSubscribers.Where(s => s.CreateDate < endDate);
            }

            return await queryableSubscribers.Where(s => s.IsDeleted == 0).CountAsync();
        }

        public async Task UpdateHubSpotDetails(Guid subscriberId, long hubSpotVid)
        {
            var subscriber = await _dbContext.Subscriber
                .SingleOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberId);

            await UpdateHubSpotDetails(subscriber, hubSpotVid);
        }

        public async Task UpdateHubSpotDetails(int subscriberId, long hubSpotVid)
        {
            var subscriber = await _dbContext.Subscriber
                .SingleOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId);

            await UpdateHubSpotDetails(subscriber, hubSpotVid);
        }

        private async Task UpdateHubSpotDetails(Subscriber subscriber, long hubSpotVid)
        {
            subscriber.HubSpotVid = hubSpotVid;
            subscriber.HubSpotModifyDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCandidateEmploymentPreferencesBySubscriberGuidAsync(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto)
        {
            var subscriber = _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .Include(s => s.SubscriberEmploymentTypes)
                              .Include(s => s.CommuteDistance)
                              .FirstOrDefault();

            var employmentTypes = _dbContext.EmploymentType
                .Where(et => candidateEmploymentPreferenceDto.EmploymentTypeGuids != null &&
                            candidateEmploymentPreferenceDto.EmploymentTypeGuids.Count > 0 &&
                            candidateEmploymentPreferenceDto.EmploymentTypeGuids.Contains(et.EmploymentTypeGuid))
                .ToList();

            var commuteDistance = _dbContext.CommuteDistance
                .FirstOrDefault(cd => candidateEmploymentPreferenceDto.CommuteDistanceGuid.HasValue &&
                                      cd.CommuteDistanceGuid == candidateEmploymentPreferenceDto.CommuteDistanceGuid);

            subscriber.IsFlexibleWorkScheduleRequired = candidateEmploymentPreferenceDto.IsFlexibleWorkScheduleRequired;
            subscriber.IsWillingToTravel = candidateEmploymentPreferenceDto.IsWillingToTravel;
            subscriber.CommuteDistanceId = commuteDistance?.CommuteDistanceId;

            var employmentTypeIds = employmentTypes.Select(et => et.EmploymentTypeId).ToList();

            var newSubscriberEmploymentTypes = new List<SubscriberEmploymentTypes>();
            if(employmentTypes != null && employmentTypes.Count > 0)
            {
                if(subscriber.SubscriberEmploymentTypes.Count != employmentTypeIds.Count || 
                   subscriber.SubscriberEmploymentTypes.Any(set => !employmentTypeIds.Contains(set.EmploymentTypeId)))
                {
                    foreach (var et in employmentTypes)
                    {
                        newSubscriberEmploymentTypes.Add(new SubscriberEmploymentTypes
                        {
                            SubscriberEmploymentTypesGuid = Guid.NewGuid(),
                            SubscriberId = subscriber.SubscriberId,
                            EmploymentTypeId = et.EmploymentTypeId
                        });

                    }

                    subscriber.SubscriberEmploymentTypes = newSubscriberEmploymentTypes;
                }

            }
            else
            {
                subscriber.SubscriberEmploymentTypes = null;
            }

            _dbContext.SaveChangesAsync();
        }
    }
}
