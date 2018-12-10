using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    [Obsolete("why do we need a separate dto for subscriber creation?", false)]
    public class SubscriberCreateDto : BaseDto
    {
        public string SubscriberGuid { get; set; }
        public string SubscriberEmail { get; set; }
    }
}
