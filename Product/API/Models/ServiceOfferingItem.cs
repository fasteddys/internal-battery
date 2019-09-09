using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ServiceOfferingItem : BaseModel
    {
        public int ServiceOfferingItemId { get; set; }
        public Guid ServiceOfferingItemGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ServiceOfferingId { get; set; }
        public virtual ServiceOffering ServiceOffering { get; set; }
        public int SortOrder { get; set; }
    }
}
