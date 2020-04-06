using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CommentRepository : UpDiddyRepositoryBase<ProfileComment>, ICommentRepository
    {
        private UpDiddyDbContext _dbContext;

        public CommentRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProfileComment> GetCommentForRecruiter(Guid commentGuid, Guid subscriberGuid)
        {
            var isRecruiterInCompanyProfile = (from s in _dbContext.Subscriber
                                               join r in _dbContext.Recruiter on s.SubscriberId equals r.SubscriberId
                                               join rc in _dbContext.RecruiterCompany on r.RecruiterId equals rc.RecruiterId
                                               join p in _dbContext.Profile on rc.CompanyId equals p.CompanyId
                                               join pc in _dbContext.ProfileComment on p.ProfileId equals pc.ProfileId
                                               where pc.ProfileCommentGuid == commentGuid && s.SubscriberGuid == subscriberGuid
                                               select p).Any();
            if (!isRecruiterInCompanyProfile)
                throw new FailedValidationException("recruiter does not belong to the company of the profile associated with the comment");

            var recruiterComment = await (from c in _dbContext.ProfileComment.Include(c => c.Recruiter).Include(c => c.Profile)
                                          join r in _dbContext.Recruiter on c.RecruiterId equals r.RecruiterId
                                          join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                          where c.ProfileCommentGuid == commentGuid && c.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid
                                          select c).FirstOrDefaultAsync();
            if (recruiterComment == null)
            {
                var visibleAcrossCompanyComment = await (from c in _dbContext.ProfileComment.Include(c => c.Recruiter).Include(c => c.Profile)
                                                         join p in _dbContext.Profile on c.ProfileId equals p.ProfileId
                                                         join co in _dbContext.Company on p.CompanyId equals co.CompanyId
                                                         join rc in _dbContext.RecruiterCompany on co.CompanyId equals rc.CompanyId
                                                         join r in _dbContext.Recruiter on rc.RecruiterId equals r.RecruiterId
                                                         join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                         where c.ProfileCommentGuid == commentGuid && c.IsDeleted == 0 && c.IsVisibleToCompany
                                                         select c).FirstOrDefaultAsync();
                return visibleAcrossCompanyComment;
            }
            else
            {
                return recruiterComment;
            }
        }

        public async Task<List<CommentDto>> GetProfileCommentsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@ProfileGuid", profileGuid),
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<CommentDto> comments = null;
            comments = await _dbContext.Comments.FromSql<CommentDto>("[G2].[System_Get_ProfileCommentsForRecruiter] @ProfileGuid, @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return comments;
        }

        public async Task<Guid> CreateCommentForRecruiter(Guid subscriberGuid, CommentDto commentDto)
        {

            var isRecruiterInCompanyProfile = (from s in _dbContext.Subscriber
                                               join r in _dbContext.Recruiter on s.SubscriberId equals r.SubscriberId
                                               join rc in _dbContext.RecruiterCompany on r.RecruiterId equals rc.RecruiterId
                                               join p in _dbContext.Profile on rc.CompanyId equals p.CompanyId
                                               where p.ProfileGuid == commentDto.ProfileGuid && s.SubscriberGuid == subscriberGuid && p.IsDeleted == 0
                                               select p).Any();
            if (!isRecruiterInCompanyProfile)
                throw new FailedValidationException("recruiter does not belong to the company of the profile associated with the comment");

            Guid commentGuid = Guid.NewGuid();
            var recruiterId = (from s in _dbContext.Subscriber
                               join r in _dbContext.Recruiter on s.SubscriberId equals r.SubscriberId
                               where s.SubscriberGuid == subscriberGuid && r.IsDeleted == 0
                               select r.RecruiterId).FirstOrDefault();
            if (recruiterId == 0)
                throw new FailedValidationException("recruiter not found");
            var profileId = (from p in _dbContext.Profile
                             where p.ProfileGuid == commentDto.ProfileGuid && p.IsDeleted == 0
                             select p.ProfileId).FirstOrDefault();
            if (profileId == 0)
                throw new FailedValidationException("profile not found");
            this.Create(new ProfileComment()
            {
                CreateDate = DateTime.UtcNow,
                CreateGuid = Guid.Empty,
                Value = commentDto.Value,
                IsDeleted = 0,
                ProfileCommentGuid = commentGuid,
                IsVisibleToCompany = commentDto.IsVisibleToCompany,
                RecruiterId = recruiterId,
                ProfileId = profileId
            });
            await this.SaveAsync();
            return commentGuid;
        }

        public async Task<(Recruiter recruiter, List<Profile> profiles)> GetValidProfiles(Guid subscriberGuid, List<Guid> profileGuid)
        {
            var recruiter = await _dbContext.Recruiter
                .Include(r => r.Subscriber)
                .Include(r => r.RecruiterCompanies).ThenInclude(rc => rc.Company)
                .Where(r => r.IsDeleted == 0 && r.Subscriber.SubscriberGuid == subscriberGuid)
                .FirstOrDefaultAsync();

            if (recruiter == null)
            {
                throw new FailedValidationException("recruiter not found");
            }

            var validCompanies = recruiter.RecruiterCompanies
                .Select(rc => rc?.CompanyId)
                .Where(c => c != null)
                .ToList();

            var validProfiles = await _dbContext.Profile
                .Where(p => p.IsDeleted == 0 && profileGuid.Contains(p.ProfileGuid) && validCompanies.Contains(p.CompanyId))
                .ToListAsync();

            return (recruiter, validProfiles);
        }

        public async Task UpdateCommentForRecruiter(Guid subscriberGuid, CommentDto commentDto)
        {
            var isRecruiterInCompanyProfile = (from s in _dbContext.Subscriber
                                               join r in _dbContext.Recruiter on s.SubscriberId equals r.SubscriberId
                                               join rc in _dbContext.RecruiterCompany on r.RecruiterId equals rc.RecruiterId
                                               join p in _dbContext.Profile on rc.CompanyId equals p.CompanyId
                                               join pc in _dbContext.ProfileComment on p.ProfileId equals pc.ProfileId
                                               where pc.ProfileCommentGuid == commentDto.CommentGuid && s.SubscriberGuid == subscriberGuid
                                               select p).Any();
            if (!isRecruiterInCompanyProfile)
                throw new FailedValidationException("recruiter does not belong to the company of the profile associated with the comment");

            var comment = (from c in _dbContext.ProfileComment
                           join r in _dbContext.Recruiter on c.RecruiterId equals r.RecruiterId
                           join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                           where s.SubscriberGuid == subscriberGuid && c.ProfileCommentGuid == commentDto.CommentGuid && c.IsDeleted == 0
                           select c).FirstOrDefault();
            if (comment == null)
                throw new NotFoundException("comment not found");
            comment.ModifyDate = DateTime.UtcNow;
            comment.ModifyGuid = Guid.Empty;
            comment.Value = commentDto.Value;
            comment.IsVisibleToCompany = commentDto.IsVisibleToCompany;
            this.Update(comment);
            await this.SaveAsync();
        }

        public async Task DeleteCommentForRecruiter(Guid subscriberGuid, Guid commentGuid)
        {
            var isRecruiterInCompanyProfile = (from s in _dbContext.Subscriber
                                               join r in _dbContext.Recruiter on s.SubscriberId equals r.SubscriberId
                                               join rc in _dbContext.RecruiterCompany on r.RecruiterId equals rc.RecruiterId
                                               join p in _dbContext.Profile on rc.CompanyId equals p.CompanyId
                                               join pc in _dbContext.ProfileComment on p.ProfileId equals pc.ProfileId
                                               where pc.ProfileCommentGuid == commentGuid && s.SubscriberGuid == subscriberGuid
                                               select p).Any();
            if (!isRecruiterInCompanyProfile)
                throw new FailedValidationException("recruiter does not belong to the company of the profile associated with the comment");

            var comment = (from c in _dbContext.ProfileComment
                           join r in _dbContext.Recruiter on c.RecruiterId equals r.RecruiterId
                           join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                           where s.SubscriberGuid == subscriberGuid && c.ProfileCommentGuid == commentGuid && c.IsDeleted == 0
                           select c).FirstOrDefault();
            if (comment == null)
                throw new NotFoundException("comment not found");
            comment.ModifyDate = DateTime.UtcNow;
            comment.ModifyGuid = Guid.Empty;
            comment.IsDeleted = 1;
            this.Update(comment);
            await this.SaveAsync();
        }
    }
}
