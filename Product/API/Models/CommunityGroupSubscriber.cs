using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using UpDiddyApi.ApplicationCore.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyApi.Models
{
    public class CommunityGroupSubscriber : BaseModel
    {
        public int CommunityGroupSubscriberId { get; set; }
        public Guid? CommunityGroupSubscriberGuid { get; set; }
        public int CommunityGroupId { get; set; }
        public int SubscriberId { get; set; }
        public Subscriber Subscriber { get; set; }
        public CommunityGroup CommunityGroup { get; set; }
    }
}