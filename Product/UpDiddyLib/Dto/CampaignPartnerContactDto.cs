using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CampaignPartnerContactDto : BaseDto
    {
        public Guid CampaignGuid { get; set; }
        public bool IsCampaignActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Guid PartnerContactGuid { get; set; }
        public string TargetedViewName { get; set; }
    }
}
