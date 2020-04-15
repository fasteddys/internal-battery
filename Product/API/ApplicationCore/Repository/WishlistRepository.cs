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

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class WishlistRepository : UpDiddyRepositoryBase<Wishlist>, IWishlistRepository
    {
        private UpDiddyDbContext _dbContext;

        public WishlistRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Wishlist> GetWishlistForRecruiter(Guid wishlistGuid, Guid subscriberGuid)
        {
            return await (from w in _dbContext.Wishlist.Include(w => w.Recruiter)
                          join r in _dbContext.Recruiter on w.RecruiterId equals r.RecruiterId
                          join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                          where w.WishlistGuid == wishlistGuid && s.SubscriberGuid == subscriberGuid && w.IsDeleted == 0
                          select w)
                          .FirstOrDefaultAsync();
        }

        public async Task<List<ProfileWishlistDto>> GetProfileWishlistsForRecruiter(Guid wishlistGuid, Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@WishlistGuid", wishlistGuid),
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<ProfileWishlistDto> profileWishlists = null;
            profileWishlists = await _dbContext.ProfileWishlists.FromSql<ProfileWishlistDto>("[G2].[System_Get_ProfileWishlistsForRecruiter] @WishlistGuid, @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return profileWishlists;
        }

        public async Task<Guid> CreateWishlistForRecruiter(Guid subscriberGuid, WishlistDto wishlistDto)
        {
            Guid wishlistGuid = Guid.NewGuid();
            var recruiterId = (from s in _dbContext.Subscriber
                               join r in _dbContext.Recruiter on s.SubscriberId equals r.SubscriberId
                               where s.SubscriberGuid == subscriberGuid
                               select r.RecruiterId).FirstOrDefault();

            wishlistDto.Name = GetAutoIncrementedWishlistName(wishlistDto.Name, subscriberGuid, wishlistDto.WishlistGuid);

            this.Create(new Wishlist()
            {
                CreateDate = DateTime.UtcNow,
                CreateGuid = Guid.Empty,
                Description = wishlistDto.Description,
                IsDeleted = 0,
                Name = wishlistDto.Name,
                WishlistGuid = wishlistGuid,
                RecruiterId = recruiterId
            });
            await this.SaveAsync();
            return wishlistGuid;
        }

        public async Task UpdateWishlistForRecruiter(Guid subscriberGuid, WishlistDto wishlistDto)
        {
            bool isWishlistOwnedBySubscriber = (from w in _dbContext.Wishlist
                                                join r in _dbContext.Recruiter on w.RecruiterId equals r.RecruiterId
                                                join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                where s.SubscriberGuid == subscriberGuid && w.WishlistGuid == wishlistDto.WishlistGuid
                                                select w.WishlistId).Any();
            if (!isWishlistOwnedBySubscriber)
                throw new FailedValidationException($"recruiter does not have permission to modify wishlist");

            var wishlist = (from w in _dbContext.Wishlist
                            where w.WishlistGuid == wishlistDto.WishlistGuid && w.IsDeleted == 0
                            select w).FirstOrDefault();
            if (wishlist == null)
                throw new NotFoundException("wishlist not found");

            wishlist.ModifyDate = DateTime.UtcNow;
            wishlist.ModifyGuid = Guid.Empty;
            wishlist.Name = GetAutoIncrementedWishlistName(wishlistDto.Name, subscriberGuid, wishlistDto.WishlistGuid);
            wishlist.Description = wishlistDto.Description;
            this.Update(wishlist);
            await this.SaveAsync();
        }

        public async Task DeleteWishlistForRecruiter(Guid subscriberGuid, Guid wishlistGuid)
        {
            bool isWishlistOwnedBySubscriber = (from w in _dbContext.Wishlist
                                                join r in _dbContext.Recruiter on w.RecruiterId equals r.RecruiterId
                                                join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                where s.SubscriberGuid == subscriberGuid && w.WishlistGuid == wishlistGuid
                                                select w.WishlistId).Any();
            if (!isWishlistOwnedBySubscriber)
                throw new FailedValidationException($"recruiter does not have permission to modify wishlist");

            var wishlist = (from w in _dbContext.Wishlist
                            where w.WishlistGuid == wishlistGuid && w.IsDeleted == 0
                            select w).FirstOrDefault();
            if (wishlist == null)
                throw new NotFoundException("wishlist not found");

            wishlist.ModifyDate = DateTime.UtcNow;
            wishlist.ModifyGuid = Guid.Empty;
            wishlist.IsDeleted = 1;
            this.Update(wishlist);
            await this.SaveAsync();
        }

        public async Task<List<WishlistDto>> GetWishlistsForRecruiter(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<WishlistDto> profileWishlists = null;
            profileWishlists = await _dbContext.Wishlists.FromSql<WishlistDto>("[G2].[System_Get_WishlistsForRecruiter] @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return profileWishlists;
        }

        public async Task<List<Guid>> AddProfileWishlistsForRecruiter(Guid subscriberGuid, Guid wishlistGuid, List<Guid> profileGuids)
        {
            var wishList = await _dbContext.Wishlist
                .Include(wl => wl.Recruiter)
                .ThenInclude(r => r.Subscriber)
                .FirstOrDefaultAsync(wl => wl.WishlistGuid == wishlistGuid && wl.IsDeleted == 0);

            if (wishList == null)
                throw new NotFoundException("Wishlist not found");

            if (wishList.Recruiter?.Subscriber?.SubscriberGuid != subscriberGuid)
                throw new FailedValidationException($"recruiter does not have permission to modify wishlist");

            var profiles = await _dbContext.Profile
                .Where(p => p.IsDeleted == 0 && profileGuids.Contains(p.ProfileGuid))
                .ToListAsync();

            var profileWishlistGuids = new List<Guid>();
            foreach (var profile in profiles)
            {
                var existingWishListProfile = await _dbContext.ProfileWishlist
                    .FirstOrDefaultAsync(pwl => pwl.ProfileId == profile.ProfileId && pwl.WishlistId == wishList.WishlistId);

                if (existingWishListProfile == null)
                {
                    var newWishListItem = new ProfileWishlist
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        ProfileId = profile.ProfileId,
                        WishlistId = wishList.WishlistId,
                        ProfileWishlistGuid = Guid.NewGuid()
                    };
                    _dbContext.ProfileWishlist.Add(newWishListItem);
                    profileWishlistGuids.Add(newWishListItem.ProfileWishlistGuid);
                }
                else
                {
                    if (existingWishListProfile.IsDeleted == 1)
                    {
                        existingWishListProfile.IsDeleted = 0;
                        existingWishListProfile.ModifyDate = DateTime.UtcNow;

                        profileWishlistGuids.Add(existingWishListProfile.ProfileWishlistGuid);
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
            return profileWishlistGuids;
        }

        public async Task DeleteProfileWishlistsForRecruiter(Guid subscriberGuid, List<Guid> profileWishlistGuids)
        {
            bool isWishlistOwnedBySubscriber = (from pw in _dbContext.ProfileWishlist
                                                join w in _dbContext.Wishlist on pw.WishlistId equals w.WishlistId
                                                join r in _dbContext.Recruiter on w.RecruiterId equals r.RecruiterId
                                                join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                where s.SubscriberGuid == subscriberGuid && pw.ProfileWishlistGuid == profileWishlistGuids.FirstOrDefault()
                                                select pw.ProfileWishlistId).Any();
            if (!isWishlistOwnedBySubscriber)
                throw new FailedValidationException($"recruiter does not have permission to modify wishlist");

            foreach (Guid profileWishlistGuid in profileWishlistGuids)
            {
                var profileWishlist = (from pw in _dbContext.ProfileWishlist
                                       where pw.ProfileWishlistGuid == profileWishlistGuid && pw.IsDeleted == 0
                                       select pw).FirstOrDefault();
                if (profileWishlist == null)
                    throw new NotFoundException($"profile wishlist '{profileWishlistGuid}' not found");

                profileWishlist.ModifyDate = DateTime.UtcNow;
                profileWishlist.ModifyGuid = Guid.Empty;
                profileWishlist.IsDeleted = 1;
                _dbContext.Update(profileWishlist);
            }
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// This method was created to satisfy the requirements here: https://allegisdigital.visualstudio.com/UpDiddy/_workitems/edit/2044
        /// It may seem overly complicated at first glance; please consider the following cases which needed to be considered and handled:
        ///     1. Create Wishlist - requested name exists (exact match)
        ///     2. Create Wishlist - requested name exists and there are other auto-incremented variations of it too
        ///     3. Create Wishlist - requested name has been logically deleted name(exact match)
        ///     4. Create Wishlist - requested name has been logically deleted with auto-incremented variations 
        ///     5. Create Wishlist - requested name exists with auto-increment value(exact match)
        ///     6. Update Wishlist - requested name exists for another wishlist guid(exact match)
        ///     7. Update Wishlist - requested name exists for another wishlist guid and there are other auto-incremented variations of it too
        ///     8. Update Wishlist - requested name exists for another wishlist guid that has been logically deleted name(exact match)
        ///     9. Update Wishlist - requested name exists for another wishlist guid that has been logically deleted with auto-incremented variations 
        ///     10. Update Wishlist - requested name exists for another wishlist guid that has an auto-increment value(exact match)
        /// </summary>
        /// <param name="wishlistName"></param>
        /// <param name="subscriberGuid"></param>
        /// <param name="wishlistGuid"></param>
        /// <returns>A wishlist name that has been adjusted to avoid back-end validation</returns>
        private string GetAutoIncrementedWishlistName(string wishlistName, Guid subscriberGuid, Guid wishlistGuid)
        {
            // check if there is an active wishlist for the recruiter with the same exact name that is being requested for creation
            var duplicateWishlistByNameForRecruiter = (from w in _dbContext.Wishlist
                                                       join r in _dbContext.Recruiter on w.RecruiterId equals r.RecruiterId
                                                       join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                       where s.SubscriberGuid == subscriberGuid && w.Name == wishlistName && w.IsDeleted == 0 && w.WishlistGuid != wishlistGuid
                                                       select w).FirstOrDefault();

            // if a duplicate exists, make changes to the requested wishlist name
            if (duplicateWishlistByNameForRecruiter != null)
            {
                int autoIncrementValue = 1;
                Regex autoIncrement = new Regex(@"\([0-9]+\)$");

                // check to see if wishlists were created on behalf of the user that have auto-incremented values appended to their name
                var wishlistsThatContainRequestedName = (from w in _dbContext.Wishlist
                                                         join r in _dbContext.Recruiter on w.RecruiterId equals r.RecruiterId
                                                         join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                         where s.SubscriberGuid == subscriberGuid && w.Name.Contains(wishlistName) && w.IsDeleted == 0 && w.WishlistGuid != wishlistGuid
                                                         select w).ToList();

                // if there are any active wishlists for the recruiter which have auto-incremented values appended to them, pick the one with the highest number, add one to it, and use that for the new auto increment value
                foreach (var wishlist in wishlistsThatContainRequestedName)
                {
                    if (autoIncrement.IsMatch(wishlist.Name))
                    {
                        int existingAutoIncrementValue = 1;
                        var firstAutoIncrementMatch = autoIncrement.Match(wishlist.Name);
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
                if (autoIncrement.IsMatch(wishlistName))
                {
                    var firstAutoIncrementMatch = autoIncrement.Match(wishlistName);

                    var numberOnly = firstAutoIncrementMatch.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    if (int.TryParse(numberOnly, out newAutoIncrementValue) && newAutoIncrementValue >= autoIncrementValue)
                        autoIncrementValue = ++newAutoIncrementValue;
                    else
                        autoIncrementValue++;

                    wishlistName = wishlistName.Replace(firstAutoIncrementMatch.Value, $"({autoIncrementValue.ToString()})");
                }
                else
                {
                    wishlistName += $" ({autoIncrementValue.ToString()})";
                }
            }

            return wishlistName;
        }
    }
}