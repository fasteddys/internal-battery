using System;

namespace UpDiddyLib.Dto
{
    public class FailedSubscriberDto : BaseDto
    {
        public Guid SubscriberGuid { get; set; }
        public string FirstName {get;set;}
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CloudTalentIndexInfo { get; set; }
    }
}
