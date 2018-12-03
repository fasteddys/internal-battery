using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class SubscriberProfileStagingStore : BaseModel
    {
        public int SubscriberProfileStagingStoreId { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }        
        public string ProfileData { get; set; }
        public string ProfileSource { get; set; }
        public string ProfileFormat { get; set; }
        public int Status { get; set; }
    }
}
