using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IOfferService
    {
        Task<List<OfferDto>> GetAllOffers(int limit = 5, int offset = 0);
        Task<OfferDto> GetOffer(Guid offerGuid);
        Task ClaimOffer(Guid subscriberGuid, Guid offerGuid);
        Task CreateOffer(OfferDto offerDto);
        Task UpdateOffer(Guid offerGuid, OfferDto offerDto);
        Task DeleteOffer(Guid offerGuid);
    }
}
