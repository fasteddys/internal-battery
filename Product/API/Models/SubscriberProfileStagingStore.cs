using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberProfileStagingStore : BaseModel
    {
        public int SubscriberProfileStagingStoreId { get; set; }
        public Guid SubscriberGuid { get; set; }
        public string ProfileData { get; set; }
        public string ProfileSource { get; set; }
        public string ProfileFormat { get; set; }
        public int Status { get; set; }
    }
}
