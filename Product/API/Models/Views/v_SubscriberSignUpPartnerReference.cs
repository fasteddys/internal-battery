using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.Views
{
    public class v_SubscriberSignUpPartnerReference
    {
        public int SubscriberId { get; set; }
        public int? PartnerId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public virtual Partner Partner { get; set; }
    }
}
