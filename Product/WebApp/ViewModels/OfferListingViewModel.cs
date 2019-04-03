using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class OfferListingViewModel : BaseViewModel
    {
        public IList<OfferDto> Offers { get; set; }
        public bool IsEligible { get; set; }
    }
}
