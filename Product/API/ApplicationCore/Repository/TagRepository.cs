using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.Models.G2;
using System.Data.SqlClient;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class TagRepository : UpDiddyRepositoryBase<Tag>, ITagRepository
    {
        private UpDiddyDbContext _dbContext;
 

        public TagRepository(UpDiddyDbContext dbContext) : base(dbContext)
       {
            _dbContext = dbContext;
        }

        public async Task<List<TagDto>> GetTags(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<TagDto> rval = null;
            rval = await _dbContext.Tags.FromSql<TagDto>("System_Get_Tags @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<ProfileTagDto>> GetProfileTagsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@ProfileGuid", profileGuid),
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<ProfileTagDto> profileTags = null;
            profileTags = await _dbContext.ProfileTags.FromSql<ProfileTagDto>("[G2].[System_Get_ProfileTagsForRecruiter] @ProfileGuid, @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return profileTags;
        }

        public async Task<List<Guid>> AddTagsToProfileForRecruiter(Guid subscriberGuid, List<Guid> tagGuids, Guid profileGuid)
        {
            bool isProfileAssociatedWithRecruiterCompany = (from p in _dbContext.Profile
                                                            join c in _dbContext.Company on p.CompanyId equals c.CompanyId
                                                            join rc in _dbContext.RecruiterCompany on c.CompanyId equals rc.CompanyId
                                                            join r in _dbContext.Recruiter on rc.RecruiterId equals r.RecruiterId
                                                            join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                            where s.SubscriberGuid == subscriberGuid && p.ProfileGuid == profileGuid
                                                            select rc.RecruiterCompanyId).Any();
            if (!isProfileAssociatedWithRecruiterCompany)
                throw new UnauthorizedAccessException($"TagRepository:AddTagsToProfileForRecruiter Recruiter does not have permission to modify profile tags");

            List<Guid> profileTagGuids = new List<Guid>();
            foreach (Guid tagGuid in tagGuids)
            { 
                bool isTagAlreadyAddedProfile = (from ps in _dbContext.ProfileTag
                                                   join p in _dbContext.Profile on ps.ProfileId equals p.ProfileId
                                                   join s in _dbContext.Tag on ps.TagId equals s.TagId
                                                   where s.TagGuid == tagGuid && p.ProfileGuid == profileGuid
                                                   select ps.ProfileTagId).Any();
                // Per Brent: make this method idempotent and not throw an error if the tag has already been added 
                if (isTagAlreadyAddedProfile)
                    continue;

                Guid profileTagGuid = Guid.NewGuid();
                profileTagGuids.Add(profileTagGuid);

                var profileId = (from p in _dbContext.Profile
                                 where p.ProfileGuid == profileGuid && p.IsDeleted == 0
                                 select p.ProfileId).FirstOrDefault();
                if (profileId == null || profileId == 0)
                    throw new FailedValidationException("Profile not found");

                var tagId = (from s in _dbContext.Tag
                               where s.TagGuid == tagGuid && s.IsDeleted == 0
                               select s.TagId).FirstOrDefault();
                if (tagId == null || tagId == 0)
                    throw new FailedValidationException("Tag not found");

                ProfileTag tagDeletedFromProfile = (from ps in _dbContext.ProfileTag
                                                        join p in _dbContext.Profile on ps.ProfileId equals p.ProfileId
                                                        join s in _dbContext.Tag on ps.TagId equals s.TagId
                                                        where s.TagGuid == tagGuid && p.ProfileGuid == profileGuid && ps.IsDeleted == 1
                                                        select ps).FirstOrDefault();

                if (tagDeletedFromProfile != null)
                {
                    tagDeletedFromProfile.IsDeleted = 0;
                    tagDeletedFromProfile.ModifyDate = DateTime.UtcNow;
                    tagDeletedFromProfile.ModifyGuid = Guid.Empty;
                    _dbContext.ProfileTag.Update(tagDeletedFromProfile);
                }
                else
                {
                    _dbContext.ProfileTag.Add(new ProfileTag()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        ProfileId = profileId,
                        TagId = tagId,
                        ProfileTagGuid = profileTagGuid
                    });
                }
            }
            await _dbContext.SaveChangesAsync();
            return profileTagGuids;
        }

        public async Task DeleteTagsFromProfileForRecruiter(Guid subscriberGuid, List<Guid> profileTagGuids)
        {
            bool isProfileAssociatedWithRecruiterCompany = (from ps in _dbContext.ProfileTag
                                                            join p in _dbContext.Profile on ps.ProfileId equals p.ProfileId
                                                            join c in _dbContext.Company on p.CompanyId equals c.CompanyId
                                                            join rc in _dbContext.RecruiterCompany on c.CompanyId equals rc.CompanyId
                                                            join r in _dbContext.Recruiter on rc.RecruiterId equals r.RecruiterId
                                                            join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                            where s.SubscriberGuid == subscriberGuid && ps.ProfileTagGuid == profileTagGuids.FirstOrDefault()
                                                            select rc.RecruiterCompanyId).Any();
            if (!isProfileAssociatedWithRecruiterCompany)
                throw new UnauthorizedAccessException($"Recruiter does not have permission to modify profile tags");

            foreach (Guid profileTagGuid in profileTagGuids)
            {
                var profileTag = (from ps in _dbContext.ProfileTag
                                    where ps.ProfileTagGuid == profileTagGuid && ps.IsDeleted == 0
                                    select ps).FirstOrDefault();
                if (profileTag == null)
                    throw new NotFoundException($"Profile tag '{profileTagGuid}' not found");

                profileTag.ModifyDate = DateTime.UtcNow;
                profileTag.ModifyGuid = Guid.Empty;
                profileTag.IsDeleted = 1;
                _dbContext.Update(profileTag);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid> GetProfileGuidByProfileTagGuids(List<Guid> profileTagGuids)
        {
            return await (from ps in _dbContext.ProfileTag.Where(x => profileTagGuids.Contains(x.ProfileTagGuid))
                          join p in _dbContext.Profile on ps.ProfileId equals p.ProfileId
                          select p.ProfileGuid).FirstOrDefaultAsync();
        }
    }
}