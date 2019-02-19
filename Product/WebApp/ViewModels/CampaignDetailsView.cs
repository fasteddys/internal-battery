using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class CampaignDetailsViewModel
    {
        public Guid CampaignGuid { get; set; }
       public IList<CampaignDetailDto> CampaignDetails { get; set; }
    }
}
