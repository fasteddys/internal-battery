using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class RequestMetadata
    {
      
        public virtual DeviceInfo DeviceInfo { get; set; }
 
        public virtual string Domain { get; set; }
 
        public virtual string SessionId { get; set; }
 
        public virtual string UserId { get; set; }
 
        public bool AllowMissingIds { get; set;  }

    }
}
