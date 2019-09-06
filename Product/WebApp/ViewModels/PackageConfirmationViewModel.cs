using UpDiddy.ViewModels.ButterCMS;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class PackageConfirmationViewModel
    {
        public PackageServiceViewModel PackageServiceViewModel {get; set;}
        public ServiceOfferingOrderDto ServiceOfferingOrderDto { get; set; }
    }
}