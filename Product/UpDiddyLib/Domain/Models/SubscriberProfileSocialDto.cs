using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberProfileSocialDto
    {
        public Guid SubscriberGuid { get; set; }
        public string LinkedInUrl { get; set;  }
        public string GithubUrl { get; set; }
        public string FacebookUrl { get; set; }

        public string StackOverflowUrl { get; set; }

        public string TwitterUrl { get; set; }

        public DateTime? LinkedInSyncDate { get; set; }

        public string LinkedInAvatarUrl { get; set; }

    }
}
