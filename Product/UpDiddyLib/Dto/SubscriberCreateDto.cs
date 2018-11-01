using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberCreateDto : BaseDto
    {
        public string SubscriberGuid { get; set; }
        public string SubscriberEmail { get; set; }
    }
}
