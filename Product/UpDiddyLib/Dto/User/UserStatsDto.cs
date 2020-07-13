using System;
using System.Collections.Generic;

namespace UpDiddyLib.Dto.User
{
    public class UserStatsDto
    {
        public Guid SubscriberGuid { get; set; }

        public string Email { get; set; }

        public DateTime CreateDate { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime LastSignIn { get; set; }

        public string Auto0UserId { get; set; }

        public bool IsHiringManager { get; set; }

        public bool IsEmailVerified { get; set; }
    }
}
