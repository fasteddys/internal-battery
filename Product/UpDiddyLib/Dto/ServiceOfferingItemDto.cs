using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ServiceOfferingItemDto : BaseDto
    {
        public Guid ServiceOfferingItemGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ServiceOfferingId { get; set; }
        public  ServiceOfferingDto ServiceOffering { get; set; }
    }
}
