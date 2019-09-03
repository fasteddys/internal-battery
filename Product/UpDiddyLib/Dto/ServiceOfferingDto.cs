using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ServiceOfferingDto : BaseDto
    {  
        public Guid ServiceOfferingGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } 
        public Decimal Price { get; set; }
        public IList<ServiceOfferingItemDto> ServiceOfferingItems { get; set; }
        public int SortOrder { get; set; }
    }
}
