using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using Microsoft.EntityFrameworkCore;
using UpDiddyLib.Domain.Models.G2;
using System.Data.SqlClient;
using UpDiddyApi.ApplicationCore.Exceptions;
using System.Text.RegularExpressions;
using UpDiddyApi.Models.B2B;
using UpDiddyLib.Domain.Models.B2B;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class PipelineRepository : UpDiddyRepositoryBase<Pipeline>, IPipelineRepository
    {
        private UpDiddyDbContext _dbContext;

        public PipelineRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Pipeline> GetPipelineForHiringManager(Guid PipelineGuid, Guid subscriberGuid)
        {
            return await (from p in _dbContext.Pipeline.Include(p => p.HiringManager)
                          join hm in _dbContext.HiringManager on p.HiringManagerId equals hm.HiringManagerId
                          join s in _dbContext.Subscriber on hm.SubscriberId equals s.SubscriberId
                          where p.PipelineGuid == PipelineGuid && s.SubscriberGuid == subscriberGuid && p.IsDeleted == 0
                          select p)
                          .FirstOrDefaultAsync();
        }

        public async Task<List<PipelineProfileDto>> GetPipelineProfilesForHiringManager(Guid PipelineGuid, Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@PipelineGuid", PipelineGuid),
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<PipelineProfileDto> pipelineProfiles = null;
            pipelineProfiles = await _dbContext.PipelineProfiles.FromSql<PipelineProfileDto>("[B2B].[System_Get_PipelineProfilesForHiringManager] @PipelineGuid, @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return pipelineProfiles;
        }

        public async Task<Guid> CreatePipelineForHiringManager(Guid subscriberGuid, PipelineDto PipelineDto)
        {
            Guid PipelineGuid = Guid.NewGuid();
            var HiringManagerId = (from s in _dbContext.Subscriber
                                   join hm in _dbContext.HiringManager on s.SubscriberId equals hm.SubscriberId
                                   where s.SubscriberGuid == subscriberGuid
                                   select hm.HiringManagerId).FirstOrDefault();

            PipelineDto.Name = GetAutoIncrementedPipelineName(PipelineDto.Name, subscriberGuid, PipelineDto.PipelineGuid);

            this.Create(new Pipeline()
            {
                CreateDate = DateTime.UtcNow,
                CreateGuid = Guid.Empty,
                Description = PipelineDto.Description,
                IsDeleted = 0,
                Name = PipelineDto.Name,
                PipelineGuid = PipelineGuid,
                HiringManagerId = HiringManagerId
            });
            await this.SaveAsync();
            return PipelineGuid;
        }

        public async Task UpdatePipelineForHiringManager(Guid subscriberGuid, PipelineDto PipelineDto)
        {
            bool isPipelineOwnedBySubscriber = (from p in _dbContext.Pipeline
                                                join hm in _dbContext.HiringManager on p.HiringManagerId equals hm.HiringManagerId
                                                join s in _dbContext.Subscriber on hm.SubscriberId equals s.SubscriberId
                                                where s.SubscriberGuid == subscriberGuid && p.PipelineGuid == PipelineDto.PipelineGuid
                                                select p.PipelineId).Any();
            if (!isPipelineOwnedBySubscriber)
                throw new FailedValidationException($"HiringManager does not have permission to modify Pipeline");

            var Pipeline = (from p in _dbContext.Pipeline
                            where p.PipelineGuid == PipelineDto.PipelineGuid && p.IsDeleted == 0
                            select p).FirstOrDefault();
            if (Pipeline == null)
                throw new NotFoundException("Pipeline not found");

            Pipeline.ModifyDate = DateTime.UtcNow;
            Pipeline.ModifyGuid = Guid.Empty;
            Pipeline.Name = GetAutoIncrementedPipelineName(PipelineDto.Name, subscriberGuid, PipelineDto.PipelineGuid);
            Pipeline.Description = PipelineDto.Description;
            this.Update(Pipeline);
            await this.SaveAsync();
        }

        public async Task DeletePipelineForHiringManager(Guid subscriberGuid, Guid PipelineGuid)
        {
            bool isPipelineOwnedBySubscriber = (from p in _dbContext.Pipeline
                                                join hm in _dbContext.HiringManager on p.HiringManagerId equals hm.HiringManagerId
                                                join s in _dbContext.Subscriber on hm.SubscriberId equals s.SubscriberId
                                                where s.SubscriberGuid == subscriberGuid && p.PipelineGuid == PipelineGuid
                                                select p.PipelineId).Any();
            if (!isPipelineOwnedBySubscriber)
                throw new FailedValidationException($"HiringManager does not have permission to modify Pipeline");

            var Pipeline = (from p in _dbContext.Pipeline
                            where p.PipelineGuid == PipelineGuid && p.IsDeleted == 0
                            select p).FirstOrDefault();
            if (Pipeline == null)
                throw new NotFoundException("Pipeline not found");

            Pipeline.ModifyDate = DateTime.UtcNow;
            Pipeline.ModifyGuid = Guid.Empty;
            Pipeline.IsDeleted = 1;
            this.Update(Pipeline);
            await this.SaveAsync();
        }

        public async Task<List<PipelineDto>> GetPipelinesForHiringManager(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<PipelineDto> pipelineProfiles = null;
            pipelineProfiles = await _dbContext.Pipelines.FromSql<PipelineDto>("[B2B].[System_Get_PipelinesForHiringManager] @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return pipelineProfiles;
        }

        public async Task<List<Guid>> AddPipelineProfilesForHiringManager(Guid subscriberGuid, Guid PipelineGuid, List<Guid> profileGuids)
        {
            var Pipeline = await _dbContext.Pipeline
                .Include(wl => wl.HiringManager)
                .ThenInclude(r => r.Subscriber)
                .FirstOrDefaultAsync(wl => wl.PipelineGuid == PipelineGuid && wl.IsDeleted == 0);

            if (Pipeline == null)
                throw new NotFoundException("Pipeline not found");

            if (Pipeline.HiringManager?.Subscriber?.SubscriberGuid != subscriberGuid)
                throw new FailedValidationException($"HiringManager does not have permission to modify Pipeline");

            var profiles = await _dbContext.Profile
                .Where(p => p.IsDeleted == 0 && profileGuids.Contains(p.ProfileGuid))
                .ToListAsync();

            var pipelineProfileGuids = new List<Guid>();
            foreach (var profile in profiles)
            {
                var existingPipelineProfile = await _dbContext.PipelineProfile
                    .FirstOrDefaultAsync(pwl => pwl.ProfileId == profile.ProfileId && pwl.PipelineId == Pipeline.PipelineId);

                if (existingPipelineProfile == null)
                {
                    var newPipelineItem = new PipelineProfile
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        ProfileId = profile.ProfileId,
                        PipelineId = Pipeline.PipelineId,
                        PipelineProfileGuid = Guid.NewGuid()
                    };
                    _dbContext.PipelineProfile.Add(newPipelineItem);
                    pipelineProfileGuids.Add(newPipelineItem.PipelineProfileGuid);
                }
                else
                {
                    if (existingPipelineProfile.IsDeleted == 1)
                    {
                        existingPipelineProfile.IsDeleted = 0;
                        existingPipelineProfile.ModifyDate = DateTime.UtcNow;

                        pipelineProfileGuids.Add(existingPipelineProfile.PipelineProfileGuid);
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
            return pipelineProfileGuids;
        }

        public async Task DeletePipelineProfilesForHiringManager(Guid subscriberGuid, List<Guid> pipelineProfileGuids)
        {
            bool isPipelineOwnedBySubscriber = (from pp in _dbContext.PipelineProfile
                                                join p in _dbContext.Pipeline on pp.PipelineId equals p.PipelineId
                                                join hm in _dbContext.HiringManager on p.HiringManagerId equals hm.HiringManagerId
                                                join s in _dbContext.Subscriber on hm.SubscriberId equals s.SubscriberId
                                                where s.SubscriberGuid == subscriberGuid && pp.PipelineProfileGuid == pipelineProfileGuids.FirstOrDefault()
                                                select pp.PipelineProfileId).Any();
            if (!isPipelineOwnedBySubscriber)
                throw new FailedValidationException($"HiringManager does not have permission to modify Pipeline");

            foreach (Guid pipelineProfileGuid in pipelineProfileGuids)
            {
                var pipelineProfile = (from pp in _dbContext.PipelineProfile
                                       where pp.PipelineProfileGuid == pipelineProfileGuid && pp.IsDeleted == 0
                                       select pp).FirstOrDefault();
                if (pipelineProfile == null)
                    throw new NotFoundException($"Profile Pipeline '{pipelineProfileGuid}' not found");

                pipelineProfile.ModifyDate = DateTime.UtcNow;
                pipelineProfile.ModifyGuid = Guid.Empty;
                pipelineProfile.IsDeleted = 1;
                _dbContext.Update(pipelineProfile);
            }
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// This method was created to satisfy the requirements here: https://allegisdigital.visualstudio.com/UpDiddy/_workitems/edit/2044
        /// It may seem overly complicated at first glance; please consider the following cases which needed to be considered and handled:
        ///     1. Create Pipeline - requested name exists (exact match)
        ///     2. Create Pipeline - requested name exists and there are other auto-incremented variations of it too
        ///     3. Create Pipeline - requested name has been logically deleted name(exact match)
        ///     4. Create Pipeline - requested name has been logically deleted with auto-incremented variations 
        ///     5. Create Pipeline - requested name exists with auto-increment value(exact match)
        ///     6. Update Pipeline - requested name exists for another Pipeline guid(exact match)
        ///     7. Update Pipeline - requested name exists for another Pipeline guid and there are other auto-incremented variations of it too
        ///     8. Update Pipeline - requested name exists for another Pipeline guid that has been logically deleted name(exact match)
        ///     9. Update Pipeline - requested name exists for another Pipeline guid that has been logically deleted with auto-incremented variations 
        ///     10. Update Pipeline - requested name exists for another Pipeline guid that has an auto-increment value(exact match)
        /// </summary>
        /// <param name="PipelineName"></param>
        /// <param name="subscriberGuid"></param>
        /// <param name="PipelineGuid"></param>
        /// <returns>A Pipeline name that has been adjusted to avoid back-end validation</returns>
        private string GetAutoIncrementedPipelineName(string PipelineName, Guid subscriberGuid, Guid PipelineGuid)
        {
            // check if there is an active Pipeline for the HiringManager with the same exact name that is being requested for creation
            var duplicatePipelineByNameForHiringManager = (from p in _dbContext.Pipeline
                                                           join hm in _dbContext.HiringManager on p.HiringManagerId equals hm.HiringManagerId
                                                           join s in _dbContext.Subscriber on hm.SubscriberId equals s.SubscriberId
                                                           where s.SubscriberGuid == subscriberGuid && p.Name == PipelineName && p.IsDeleted == 0 && p.PipelineGuid != PipelineGuid
                                                           select p).FirstOrDefault();

            // if a duplicate exists, make changes to the requested Pipeline name
            if (duplicatePipelineByNameForHiringManager != null)
            {
                int autoIncrementValue = 1;
                Regex autoIncrement = new Regex(@"\([0-9]+\)$");

                // check to see if Pipelines were created on behalf of the user that have auto-incremented values appended to their name
                var PipelinesThatContainRequestedName = (from p in _dbContext.Pipeline
                                                         join hm in _dbContext.HiringManager on p.HiringManagerId equals hm.HiringManagerId
                                                         join s in _dbContext.Subscriber on hm.SubscriberId equals s.SubscriberId
                                                         where s.SubscriberGuid == subscriberGuid && p.Name.Contains(PipelineName) && p.IsDeleted == 0 && p.PipelineGuid != PipelineGuid
                                                         select p).ToList();

                // if there are any active Pipelines for the HiringManager which have auto-incremented values appended to them, pick the one with the highest number, add one to it, and use that for the new auto increment value
                foreach (var Pipeline in PipelinesThatContainRequestedName)
                {
                    if (autoIncrement.IsMatch(Pipeline.Name))
                    {
                        int existingAutoIncrementValue = 1;
                        var firstAutoIncrementMatch = autoIncrement.Match(Pipeline.Name);
                        var numberOnly = firstAutoIncrementMatch.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                        if (int.TryParse(numberOnly, out existingAutoIncrementValue))
                        {
                            if (existingAutoIncrementValue >= autoIncrementValue)
                                autoIncrementValue = ++existingAutoIncrementValue;
                        }
                    }
                }

                // check to see if the name being requested by the user contains an auto-incremented value. if it does, increment it (taking into consideration the auto-increment logic above)
                int newAutoIncrementValue = 1;
                if (autoIncrement.IsMatch(PipelineName))
                {
                    var firstAutoIncrementMatch = autoIncrement.Match(PipelineName);

                    var numberOnly = firstAutoIncrementMatch.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    if (int.TryParse(numberOnly, out newAutoIncrementValue) && newAutoIncrementValue >= autoIncrementValue)
                        autoIncrementValue = ++newAutoIncrementValue;
                    else
                        autoIncrementValue++;

                    PipelineName = PipelineName.Replace(firstAutoIncrementMatch.Value, $"({autoIncrementValue.ToString()})");
                }
                else
                {
                    PipelineName += $" ({autoIncrementValue.ToString()})";
                }
            }

            return PipelineName;
        }
    }
}