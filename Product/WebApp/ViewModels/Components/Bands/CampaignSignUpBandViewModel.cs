using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class CampaignSignUpBandViewModel : ComponentViewModel
    {
        public string CampaignCourse { get; set; }
        public Guid ContactGuid { get; set; }
        public Guid CampaignGuid { get; set; }
        public string RibbonText { get; set; }
        public string DisclaimerText { get; set; }
        public string CampaignPhase { get; set; }
    }
}
