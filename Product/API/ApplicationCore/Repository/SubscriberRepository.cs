using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;
using UpDiddyLib.Dto;
using SkillDto = UpDiddyLib.Domain.Models.SkillDto;

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

        public async Task<RolePreferenceDto> GetRolePreference(Guid subscriberGuid)
        {
            var subscriber = await _dbContext.Subscriber
                .Include(s => s.SubscriberSkills)
                    .ThenInclude(ss => ss.Skill)
                .Include(s => s.SubscriberLinks)
                .FirstOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid);

            if (subscriber == null) { return null; }

            var rolePreference = new RolePreferenceDto
            {
                JobTitle = subscriber.Title ?? string.Empty,
                DreamJob = subscriber.DreamJob ?? string.Empty,
                WhatSetsMeApart = subscriber.CurrentRoleProficiencies ?? string.Empty,
                WhatKindOfLeader = subscriber.PreferredLeaderStyle ?? string.Empty,
                WhatKindOfTeam = subscriber.PreferredTeamType ?? string.Empty,
                VolunteerOrPassionProjects = subscriber.PassionProjectsDescription ?? string.Empty,
                SkillGuids = subscriber.SubscriberSkills
                    .Where(ss => ss.IsDeleted == 0 && ss.Skill.SkillGuid.HasValue)
                    .Select(ss => ss.Skill.SkillGuid.Value)
                    .ToList(),
                SocialLinks = subscriber.SubscriberLinks
                    .Where(sl => sl.IsDeleted == 0)
                    .Select(sl => new SocialLinksDto
                    {
                        FriendlyName = sl.Label,
                        Url = sl.Url
                    })
                    .ToList(),
                ElevatorPitch = subscriber.CoverLetter ?? string.Empty
            };

            return rolePreference;
        }

        public async Task UpdateRolePreference(Guid subscriberGuid, RolePreferenceDto rolePreference)
        {
            if (rolePreference == null) { throw new ArgumentNullException(nameof(rolePreference)); }

            var subscriber = await _dbContext.Subscriber
                .Include(s => s.SubscriberSkills)
                    .ThenInclude(ss => ss.Skill)
                .Include(s => s.SubscriberLinks)
                .FirstOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid);

            if (subscriber == null) { return; }

            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                subscriber.Title = rolePreference.JobTitle;
                subscriber.CurrentRoleProficiencies = rolePreference.DreamJob;
                subscriber.DreamJob = rolePreference.WhatSetsMeApart;
                subscriber.PreferredLeaderStyle = rolePreference.WhatKindOfLeader;
                subscriber.PreferredTeamType = rolePreference.WhatKindOfTeam;
                subscriber.PassionProjectsDescription = rolePreference.VolunteerOrPassionProjects;
                subscriber.CoverLetter = rolePreference.ElevatorPitch;

                var skillsToDelete = subscriber.SubscriberSkills
                    .Where(ss => ss.IsDeleted == 0 && ss.Skill?.SkillGuid != null && !rolePreference.SkillGuids.Contains(ss.Skill.SkillGuid.Value));

                foreach (var skillToDelete in skillsToDelete) { skillToDelete.IsDeleted = 1; }

                var skillGuidsToAdd = rolePreference.SkillGuids
                    .Where(sg => !subscriber.SubscriberSkills.Any(ss => ss.IsDeleted == 0 && ss.Skill?.SkillGuid == sg))
                    .ToList();

                if (skillGuidsToAdd.Any())
                {
                    var newSubscriberSkills = await _dbContext.Skill
                        .Where(s => s.IsDeleted == 0 && s.SkillGuid != null && skillGuidsToAdd.Contains(s.SkillGuid.Value))
                        .Select(s => new SubscriberSkill
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.NewGuid(),
                            SubscriberSkillGuid = Guid.NewGuid(),
                            IsDeleted = 0,
                            SkillId = s.SkillId,
                            SubscriberId = subscriber.SubscriberId
                        })
                        .ToListAsync();

                    await _dbContext.SubscriberSkill.AddRangeAsync(newSubscriberSkills);
                }

                foreach (var linkToUpdate in subscriber.SubscriberLinks)
                {
                    var link = rolePreference.SocialLinks
                        .FirstOrDefault(l => l.FriendlyName == linkToUpdate.Label);
                    if (link != null) { linkToUpdate.Url = link.Url; }
                }

                var linksToDelete = subscriber.SubscriberLinks
                    .Where(link => link.IsDeleted == 0 && !rolePreference.SocialLinks.Any(sl => sl.FriendlyName == link.Label));

                foreach (var linkToDelete in linksToDelete) { linkToDelete.IsDeleted = 1; }

                var linksToAdd = rolePreference.SocialLinks
                    .Where(link => !subscriber.SubscriberLinks.Any(sl => sl.IsDeleted == 0 && sl.Label == link.FriendlyName))
                    .Select(link => new SubscriberLink
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.NewGuid(),
                        SubscriberLinkGuid = Guid.NewGuid(),
                        SubscriberId = subscriber.SubscriberId,
                        IsDeleted = 0,
                        Label = link.FriendlyName,
                        Url = link.Url
                    })
                    .ToList();

                if (linksToAdd.Any())
                {
                    await _dbContext.SubscriberLinks.AddRangeAsync(linksToAdd);
                }

                await _dbContext.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            finally { transaction.Dispose(); }
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
