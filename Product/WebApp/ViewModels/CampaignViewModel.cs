using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class CampaignViewModel : BaseViewModel
    {
        public Guid CampaignGuid { get; set; }
        public Guid ContactGuid { get; set; }
        public string TrackingImgSource { get; set; }


    }
}
