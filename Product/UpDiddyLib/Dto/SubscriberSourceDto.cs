using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberSourceDto
    {

        public long? GroupRank { get; set; }
        public int SubscriberId { get; set; }
        public Guid? SubscriberGuid { get; set; }
        public string Email { get; set; }
        public string  FirstName { get; set; }
        public string LastName { get; set; }
        public long? PartnerRank { get; set; }
        public string PartnerName { get; set; }
        public Guid? PartnerGuid { get; set; }
        public string GroupName { get; set; }
        public Guid? GroupGuid { get; set; }
    }

}
