using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ServiceOffering : BaseModel
    {

        public int ServiceOfferingId { get; set; }
        public Guid ServiceOfferingGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal Price { get; set; }
        public IList<ServiceOfferingItem> ServiceOfferingItems { get; set; }
        public int SortOrder { get; set; }
    }
}
