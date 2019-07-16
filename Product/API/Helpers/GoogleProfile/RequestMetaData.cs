using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class RequestMetadata
    {
      
        public virtual DeviceInfo deviceInfo { get; set; }
 
        public virtual string domain { get; set; }
 
        public virtual string sessionId { get; set; }
 
        public virtual string userId { get; set; }
 
        public bool allowMissingIds { get; set;  }

    }
}
