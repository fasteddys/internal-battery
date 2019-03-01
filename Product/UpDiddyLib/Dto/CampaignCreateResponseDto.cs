using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CampaignCreateResponseDto
    {
        IList<Guid> InvalidContacts { get; set; }
        string TrackingImageUrl { get; set; }
        string LandingPageUrl { get; set; }
    }
}
