
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business.G2
{
    public interface IWishlistService
    {
        Task<WishlistDto> GetWishlistForRecruiter(Guid wishlistGuid, Guid subscriberGuid);
        Task<ProfileWishlistListDto> GetProfileWishlistsForRecruiter(Guid wishlistGuid, Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<Guid> CreateWishlistForRecruiter(Guid subscriberGuid, WishlistDto wishlistDto);
        Task UpdateWishlistForRecruiter(Guid subscriberGuid, WishlistDto wishlistDto);
        Task DeleteWishlistForRecruiter(Guid subscriberGuid, Guid wishlistGuid);
        Task<WishlistListDto> GetWishlistsForRecruiter(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task DeleteProfileWishlistsForRecruiter(Guid subscriberGuid, List<Guid> profileWishlistGuids);
        Task<List<Guid>> AddProfileWishlistsForRecruiter(Guid subscriberGuid, Guid wishlistGuid, List<Guid> profileGuids);
    }
}