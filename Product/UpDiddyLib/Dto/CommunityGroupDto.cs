using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CommunityGroupDto : BaseDto
    {
        public int CommunityGroupId { get; set; }
        public Guid? CommunityGroupGuid { get; set; }
        public string Name { get; set; }

        public string ExternalId { get; set; }
    
    }
}
