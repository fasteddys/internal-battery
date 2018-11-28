using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class LinkedInToken : BaseModel
    {
        public int LinkedInTokenId { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
    }
}
