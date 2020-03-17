using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IWishlistRepository : IUpDiddyRepositoryBase<Wishlist>
    {
        Task<Wishlist> GetWishlistForRecruiter(Guid wishlistGuid, Guid subscriberGuid);
        Task<List<ProfileWishlistDto>> GetProfileWishlistsForRecruiter(Guid wishlistGuid, Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<Guid> CreateWishlistForRecruiter(Guid subscriberGuid, WishlistDto wishlistDto);
        Task UpdateWishlistForRecruiter(Guid subscriberGuid, WishlistDto wishlistDto);
        Task DeleteWishlistForRecruiter(Guid subscriberGuid, Guid wishlistGuid);
        Task<List<WishlistDto>> GetWishlistsForRecruiter(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<List<Guid>> AddProfileWishlistsForRecruiter(Guid subscriberGuid, Guid wishlistGuid, List<Guid> profileGuids);
        Task DeleteProfileWishlistsForRecruiter(Guid subscriberGuid, List<Guid> profileWishlistGuids);
    }
}
