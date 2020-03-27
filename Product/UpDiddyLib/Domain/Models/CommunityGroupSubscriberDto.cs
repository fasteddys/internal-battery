using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models
{
    public class CommunityGroupSubscriberDto
    {
        public Guid CommunityGroupSubscriberGuid { get; set; }

        public Guid CommunityGroupGuid { get; set; }

        public Guid SubscriberGuid { get; set; }
   
        public int CommunityGroupId { get; set; }

        public int SubscriberId { get; set; }
    }
}


