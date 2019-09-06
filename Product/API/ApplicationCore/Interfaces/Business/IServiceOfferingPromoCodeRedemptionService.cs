using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IServiceOfferingPromoCodeRedemptionService
    {
        bool ReserveServiceOfferingPromoCode(PromoCode promoCode, ServiceOffering serviceOffering, Subscriber subscriber, decimal adjustedPrice);
        bool ClaimServiceOfferingPromoCode(PromoCode promoCode, ServiceOffering serviceOffering, Subscriber subscriber, decimal adjustedPrice);
        bool CheckAvailability(PromoCode promoCode, ServiceOffering serviceOffering );
    }
}
