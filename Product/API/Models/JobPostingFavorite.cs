using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobPostingFavorite : BaseModel
    {
     

        public Guid JobPostingFavoriteGuid { get; set; }

        public int JobPostingId { get; set; }
        public virtual JobPosting JobPosting { get; set; }

        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }


    }
}
