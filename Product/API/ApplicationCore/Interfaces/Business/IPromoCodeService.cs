using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;


namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IPromoCodeService
    {
        decimal CalculatePrice(PromoCode promoCode, decimal BasePrice);

        bool CheckSubscriberRedemptions(PromoCode promoCode, Subscriber subscriber);

        bool ValidateStartDate(PromoCode promoCode);
        bool ValidateEndDate(PromoCode promoCode);
        bool ValidateRedemptions(PromoCode promoCode);
    }
}
