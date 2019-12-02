using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class TalentFavorite : BaseModel
    {
        public Guid TalentFavoriteGuid { get; set; }
        public int TalentFavoriteId { get; set; }
        public Subscriber Talent { get; set; }
        public int TalentId { get; set; }
        public int SubscriberId { get; set; }
        public Subscriber Subscriber { get; set; }
    }
}
