using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Services.G2
{
    public class WishlistService : IWishlistService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public WishlistService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<WishlistDto> GetWishlistForRecruiter(Guid wishlistGuid, Guid subscriberGuid)
        {
            if (wishlistGuid == null || wishlistGuid == Guid.Empty)
                throw new FailedValidationException("wishlistGuid cannot be null or empty");

            WishlistDto wishlistDto;
            var wishlist = await _repositoryWrapper.WishlistRepository.GetWishlistForRecruiter(wishlistGuid, subscriberGuid);
            if (wishlist == null)
                throw new NotFoundException("wishlist not found");

            return _mapper.Map<WishlistDto>(wishlist);
        }

        public async Task<ProfileWishlistListDto> GetProfileWishlistsForRecruiter(Guid wishlistGuid, Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            if (wishlistGuid == null || wishlistGuid == Guid.Empty)
                throw new FailedValidationException("wishlistGuid cannot be null or empty");

            var profileWishlists = await _repositoryWrapper.WishlistRepository.GetProfileWishlistsForRecruiter(wishlistGuid, subscriberGuid, limit, offset, sort, order);

            if (profileWishlists == null)
                throw new NotFoundException("profile wishlists not found");
            return _mapper.Map<ProfileWishlistListDto>(profileWishlists);
        }

        public async Task<Guid> CreateWishlistForRecruiter(Guid subscriberGuid, WishlistDto wishlistDto)
        {
            if (wishlistDto == null)
                throw new FailedValidationException("wishlistDto cannot be null");

            return await _repositoryWrapper.WishlistRepository.CreateWishlistForRecruiter(subscriberGuid, wishlistDto);
        }

        public async Task UpdateWishlistForRecruiter(Guid subscriberGuid, WishlistDto wishlistDto)
        {
            if (wishlistDto == null)
                throw new FailedValidationException("wishlistDto cannot be null");

            await _repositoryWrapper.WishlistRepository.UpdateWishlistForRecruiter(subscriberGuid, wishlistDto);
        }

        public async Task DeleteWishlistForRecruiter(Guid subscriberGuid, Guid wishlistGuid)
        {
            if (wishlistGuid == null || wishlistGuid==Guid.Empty)
                throw new FailedValidationException("wishlistGuid cannot be null or empty");

            await _repositoryWrapper.WishlistRepository.DeleteWishlistForRecruiter(subscriberGuid, wishlistGuid);
        }

        public async Task<WishlistListDto> GetWishlistsForRecruiter(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var wishlists = await _repositoryWrapper.WishlistRepository.GetWishlistsForRecruiter(subscriberGuid, limit, offset, sort, order);
            if (wishlists == null)
                throw new NotFoundException("wishlists not found");
            return _mapper.Map<WishlistListDto>(wishlists);
        }

        public async Task<List<Guid>> AddProfileWishlistsForRecruiter(Guid subscriberGuid, Guid wishlistGuid, List<Guid> profileGuids)
        {
            if (wishlistGuid == null || wishlistGuid == Guid.Empty)
                throw new FailedValidationException("wishlistGuid cannot be null or empty");

            if (profileGuids == null || profileGuids.Count() == 0)
                throw new FailedValidationException("profileGuids cannot be null or empty");

            return await _repositoryWrapper.WishlistRepository.AddProfileWishlistsForRecruiter(subscriberGuid, wishlistGuid, profileGuids);
        }

        public async Task DeleteProfileWishlistsForRecruiter(Guid subscriberGuid, List<Guid> profileWishlistGuids)
        {
            if(profileWishlistGuids == null || profileWishlistGuids.Count()== 0)
                throw new FailedValidationException("profileWishlistGuids cannot be null or empty");

            await _repositoryWrapper.WishlistRepository.DeleteProfileWishlistsForRecruiter(subscriberGuid, profileWishlistGuids);
        }
    }
}
