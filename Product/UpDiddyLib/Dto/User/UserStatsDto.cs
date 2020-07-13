using System;
using System.Collections.Generic;

namespace UpDiddyLib.Dto.User
{
    public class UserStatsDto
    {
        public Guid SubscriberGuid { get; set; }
        
        public bool IsAuth0Verified { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastKnownLogin { get; set; }

        public string PartnerSource { get; set; }
    }

    public class UserStatsListDto
    {
        public List<UserStatsDto> Users { get; set; } = new List<UserStatsDto>();

        public int TotalRecords => Users?.Count ?? 0;
    }
}
