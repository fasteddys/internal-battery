using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyLib.Dto
{
    public class VendorDto : BaseDto
    {
        public int VendorId { get; set; }
        public Guid? VendorGuid { get; set; }
        public string Name { get; set; }

    }
}
