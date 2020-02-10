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
        Task<OfferListDto> GetOffers(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
        Task<OfferDto> GetOffer(Guid offerGuid);
        Task<Guid> ClaimOffer(Guid subscriberGuid, Guid offerGuid);
        Task<bool> HasSubscriberClaimedOffer(Guid subscriberGuid, Guid offerGuid);
        Task<Guid> CreateOffer(OfferDto offerDto);
        Task UpdateOffer(Guid offerGuid, OfferDto offerDto);
        Task DeleteOffer(Guid offerGuid);
    }
}
