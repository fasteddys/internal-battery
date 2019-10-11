using System;

namespace UpDiddyLib.Dto.User
{
    public class CreateUserDto : UserDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string ReferrerUrl { get; set; }

        public string JobReferralCode { get; set; }

        public Guid PartnerGuid { get; set; }

        public Guid SubscriberGuid { get; set; }
    }
}
