using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberWorkHistory : BaseModel
    {
        public int SubscriberWorkHistoryId { get; set; }
        public Guid SubscriberWorkHistoryGuid { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int IsCurrent { get; set; }
        public string Title { get; set; }
        public string JobDecription { get; set; }
        public decimal Compensation { get; set; }
        public int CompensationTypeId { get; set;}
        public CompensationType CompensationType { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

    }
}
