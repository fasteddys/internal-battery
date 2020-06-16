using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberEmploymentTypes : BaseModel
    {
        public int SubscriberEmploymentTypesId { get; set; }
        public Guid SubscriberEmploymentTypesGuid { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int EmploymentTypeId { get; set; }
        public virtual EmploymentType EmploymentType { get; set; }
    }
}
