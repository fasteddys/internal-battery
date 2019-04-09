using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class OffersViewModel : BaseViewModel
    {
        public IList<OfferDto> Offers { get; set; }
        public bool UserIsAuthenticated { get; set; }
        public bool UserHasValidatedEmail { get; set; }
        public bool UserHasUploadedResume { get; set; }
        public string CtaText { get; set; }
        public List<string> StepsRequired { get; set; } = new List<string>();
        public bool UserIsEligibleForOffers { get; set; }
    }
}
